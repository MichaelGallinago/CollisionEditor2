using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;
using DynamicData;

namespace CollisionEditor2.Models;

public class TileSet
{
    public readonly Size TileSize;

    public List<Bitmap> Tiles { get; private set; }
    public List<byte[]> WidthMap { get; private set; }
    public List<byte[]> HeightMap { get; private set; }

    public TileSet(string path, int tileWidth = 16, int tileHeight = 16,
        Size separate = new(), Size offset = new())
    {
        TileSize = new Size(tileWidth, tileHeight);

        Tiles     = new List<Bitmap>();
        WidthMap  = new List<byte[]>();
        HeightMap = new List<byte[]>();

        Bitmap tileMap = RecolorTileMap(new Bitmap(path));

        CreateTiles(tileMap, separate, offset);
    }

    private Bitmap RecolorTileMap(Bitmap tileMap)
    {
        byte[] colorValues = BeginBitmapEdit(tileMap, TileSize, out nint ptr, out BitmapData bitmapData);

        for (int i = 0; i < colorValues.Length; i++)
        {
            if (i % 4 == 3)
            {
                if (colorValues[i] > 0)
                {
                    colorValues[i] = 255;
                }
                continue;
            }
            colorValues[i] = 0;
        }

        EndBitmapEdit(tileMap, colorValues, ptr, bitmapData);

        return tileMap;
    }

    private void CreateTiles(Bitmap tileMap, Size separate, Size offset)
    {
        var cellCount = new Vector2<int>(
            (tileMap.Width  - offset.Width)  / (TileSize.Width + separate.Width),
            (tileMap.Height - offset.Height) / (TileSize.Height + separate.Height));

        for (int y = 0; y < cellCount.Y; y++)
        {
            for (int x = 0; x < cellCount.X; x++)
            {
                var tileBounds = new Rectangle(
                    x * (TileSize.Width  + separate.Width)  + offset.Width,
                    y * (TileSize.Height + separate.Height) + offset.Height,
                    TileSize.Width, TileSize.Height);

                Tiles.Add(tileMap.Clone(tileBounds, tileMap.PixelFormat));
                if (Tiles.Count == int.MaxValue)
                {
                    CreateCollisionMap();
                    return;
                }
            }
        }

        CreateCollisionMap();
    }

    private void CreateCollisionMap()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            WidthMap.Add(new byte[TileSize.Height]);
            HeightMap.Add(new byte[TileSize.Width]);

            byte[] colorValues = BeginBitmapEdit(Tiles[i], TileSize, out nint ptr, out BitmapData bitmapData);

            CalculateCollisionArrays(i, colorValues);

            EndBitmapEdit(Tiles[i], colorValues, ptr, bitmapData);
        }
    }

    private void CalculateCollisionArrays(int tileIndex, byte[] colorValues)
    {
        for (int x = 0; x < TileSize.Width; x++)
        {
            for (int y = 0; y < TileSize.Height; y++)
            {
                if (colorValues[GetAlphaIndex(x, y)] > 0)
                {
                    WidthMap[tileIndex][y]++;
                    HeightMap[tileIndex][x]++;
                }
            }
        }
    }

    public TileSet(int angleCount = 0, int tileWidth = 16, int tileHeight = 16)
    {
        TileSize = new Size(tileWidth, tileHeight);

        Tiles     = new List<Bitmap>(angleCount);
        WidthMap  = new List<byte[]>(angleCount);
        HeightMap = new List<byte[]>(angleCount);

        for (int i = 0; i < angleCount; i++)
        {
            Tiles.Add(new Bitmap(tileWidth, tileHeight));
            WidthMap.Add(new byte[tileWidth]);
            HeightMap.Add(new byte[tileHeight]);
        }
    }

    public void Save(string path, int columnCount, Size separation = new(), Size offset = new())
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var cell = new Size(TileSize.Width + separation.Width, TileSize.Height + separation.Height);
        int rowCount = (Tiles.Count & -columnCount) / columnCount;

        var tileMapSize = new Size(
            offset.Width  + columnCount * cell.Width  - separation.Width, 
            offset.Height + rowCount    * cell.Height - separation.Height);

        Bitmap tileMap = DrawTileMap(columnCount, tileMapSize, separation, offset);

        tileMap.Save(path, ImageFormat.Png);
    }

    public static void SaveCollisionMap(string path, List<byte[]> collisionMap)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var writer = new BinaryWriter(File.Open(path, FileMode.CreateNew));
        {
            foreach (byte[] values in collisionMap)
            {
                foreach (byte value in values)
                {
                    writer.Write(value);
                }
            }
        }
    }

    public Bitmap DrawTileMap(int columnCount, Size tileMapSize, Size separation, Size offset)
    {
        var tileMap = new Bitmap(tileMapSize.Width, tileMapSize.Height);

        using (var graphics = Graphics.FromImage(tileMap))
        {
            Vector2<int> position = new();
            foreach (Bitmap tile in Tiles)
            {
                graphics.DrawImage(
                tile,
                new Rectangle(
                    offset.Width  + position.X * (TileSize.Width  + separation.Width),
                    offset.Height + position.Y * (TileSize.Height + separation.Height),
                    TileSize.Width, TileSize.Height),
                new Rectangle(0, 0, TileSize.Width, TileSize.Height),
                GraphicsUnit.Pixel);

                if (++position.X >= columnCount)
                {
                    position.X = 0;
                    position.Y++;
                }
            }
        }
        return tileMap;
    }

    public void TileChangeLine(int tileIndex, Vector2<int> tilePosition, bool isLeftButtonPressed)
    {
        Bitmap tile = Tiles[tileIndex];

        byte[] colorValues = BeginBitmapEdit(tile, TileSize, out nint ptr, out BitmapData bitmapData);

        if (isLeftButtonPressed)
        {
            if (tilePosition.Y == 0 || colorValues[GetAlphaIndex(tilePosition.X, tilePosition.Y)] == 0 ||
                colorValues[GetAlphaIndex(tilePosition.X, tilePosition.Y - 1)] != 0)
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    colorValues[GetAlphaIndex(tilePosition.X, y)] = (byte)(y < tilePosition.Y ? 0 : 255);
                }
            }
            else
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    colorValues[GetAlphaIndex(tilePosition.X, y)] = 0;
                }
            }
        }
        else
        {
            int alphaOnPositionIndex = GetAlphaIndex(tilePosition.X, tilePosition.Y);
            colorValues[alphaOnPositionIndex] = (byte)(colorValues[alphaOnPositionIndex] == 0 ? 255 : 0);
        }

        WidthMap.Replace(WidthMap[tileIndex],   new byte[TileSize.Width]);
        HeightMap.Replace(HeightMap[tileIndex], new byte[TileSize.Height]);
        CalculateCollisionArrays(tileIndex, colorValues);

        EndBitmapEdit(tile, colorValues, ptr, bitmapData);
    }

    private byte[] BeginBitmapEdit(Bitmap bitmap, Size areaSize, out nint ptr, out BitmapData bitmapData)
    {
        bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, areaSize.Width, areaSize.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        ptr = bitmapData.Scan0;
        int bytes = Math.Abs(bitmapData.Stride) * areaSize.Height;
        var colorValues = new byte[bytes];

        Marshal.Copy(ptr, colorValues, 0, bytes);

        return colorValues;
    }

    private void EndBitmapEdit(Bitmap bitmap, byte[] colorValues, nint ptr, BitmapData bitmapData)
    {
        Marshal.Copy(colorValues, 0, ptr, colorValues.Length);
        bitmap.UnlockBits(bitmapData);
    }

    private int GetAlphaIndex(int positionX, int positionY)
    {
        return (positionY * TileSize.Width + positionX) * 4 + 3;
    }

    public void InsertTile(int tileIndex)
    {
        Tiles.Insert(tileIndex, new Bitmap(TileSize.Width, TileSize.Height));
    }

    public void RemoveTile(int tileIndex)
    {
        Tiles.RemoveAt(tileIndex);
    }
}
