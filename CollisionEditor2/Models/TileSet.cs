using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;

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
        BitmapData bitmapData = tileMap.LockBits(
            new Rectangle(0, 0, tileMap.Width, tileMap.Height), 
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        IntPtr ptr = bitmapData.Scan0;
        int bytes = Math.Abs(bitmapData.Stride) * TileSize.Height;
        var ColorValues = new byte[bytes];

        System.Runtime.InteropServices.Marshal.Copy(ptr, ColorValues, 0, bytes);
        for (int i = 0; i < ColorValues.Length; i++)
        {
            if (i % 4 == 3)
            {
                if (ColorValues[i] > 0)
                {
                    ColorValues[i] = 255;
                }
                continue;
            }
            ColorValues[i] = 0;
        }
        System.Runtime.InteropServices.Marshal.Copy(ColorValues, 0, ptr, bytes);

        tileMap.UnlockBits(bitmapData);

        return tileMap;
    }

    private void CreateTiles(Bitmap tileMap, Size separate, Size offset)
    {
        var cellCount = new Vector2<int>(
            (tileMap.Width  - offset.Width)  / TileSize.Width,
            (tileMap.Height - offset.Height) / TileSize.Height);

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

            for (int x = 0; x < TileSize.Width; x++)
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    if (Tiles[i].GetPixel(x, y).A > 0)
                    {
                        WidthMap[i][y]++;
                        HeightMap[i][x]++;
                    }
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
        Color CurrentPixel = tile.GetPixel(tilePosition.X, tilePosition.Y);
        Rectangle rectangle = new Rectangle(0, 0, TileSize.Width, TileSize.Height);

        if (isLeftButtonPressed)
        {
            BitmapData bitmapData = tile.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * TileSize.Height;
            byte[] argbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, bytes);

            for (int counter = 2; counter < argbValues.Length; counter += 3)
                argbValues[counter] = 255;

            System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, bytes);
            tile.UnlockBits(bitmapData);



            if (CompareColors(CurrentPixel, Color.Transparent) || tilePosition.Y == 0 ||
                !CompareColors(tile.GetPixel(tilePosition.X, tilePosition.Y - 1), Color.Transparent))
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    tile.MakeTransparent();
                    tile.SetPixel(tilePosition.X, y, y < tilePosition.Y ? Color.Transparent : Color.Black);
                    Color color = tile.GetPixel(tilePosition.X, y);
                    if (CompareColors(Color.Transparent, color))
                    {
                        Color test = Color.FromArgb(0, 0, 0, 0);
                        continue;
                    }
                }
            }
            else
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    tile.SetPixel(tilePosition.X, y, Color.Transparent);
                }
            }
        }
        else
        {
            if (CurrentPixel == Color.Transparent || CurrentPixel != Color.Transparent
                && (tilePosition.X == 0 || tile.GetPixel(tilePosition.X - 1, tilePosition.Y) != Color.Transparent))
            {
                for (int x = 0; x < TileSize.Height; x++)
                {
                    tile.SetPixel(x, tilePosition.Y, x < tilePosition.X ? Color.Transparent : Color.Black);
                }
            }
            else
            {
                for (int x = 0; x < TileSize.Height; x++)
                {
                    tile.SetPixel(x, tilePosition.Y, Color.Transparent);
                }
            }
        }
    }

    private bool CompareColors(Color first, Color second)
    {
        return first.ToArgb() == second.ToArgb();
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
