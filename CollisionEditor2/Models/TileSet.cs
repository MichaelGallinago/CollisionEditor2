using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;

namespace CollisionEditor2.Models;

public class TileSet
{
    public readonly Vector2<int> TileSize;

    public List<Tile> Tiles { get; private set; }

    public TileSet(string path, int tileWidth = 16, int tileHeight = 16,
        Vector2<int> separate = new(), Vector2<int> offset = new())
    {
        TileSize = new Vector2<int>(tileWidth, tileHeight);

        Tiles = new List<Tile>();

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

    private void CreateTiles(Bitmap tileMap, Vector2<int> separate, Vector2<int> offset)
    {
        var cellCount = new Vector2<int>(
            ((int)tileMap.Size.Width  - offset.X)  / (TileSize.X + separate.X),
            ((int)tileMap.Size.Height - offset.Y) / (TileSize.Y + separate.Y));

        for (int y = 0; y < cellCount.Y; y++)
        {
            for (int x = 0; x < cellCount.X; x++)
            {
                var tileBounds = new Rectangle(
                    x * (TileSize.X  + separate.X)  + offset.X,
                    y * (TileSize.Y + separate.Y) + offset.Y,
                    TileSize.X, TileSize.Y);

                Bitmap bitmap = tileMap.Clone(tileBounds, tileMap.PixelFormat);
                Tiles.Add(CreateTileFromBitmap(bitmap));
                if (Tiles.Count == int.MaxValue)
                {
                    return;
                }
            }
        }
    }

    private Tile CreateTileFromBitmap(Bitmap bitmap)
    {
        var tile = new Tile(TileSize);
        bool[] pixels = tile.Pixels;
        byte[] colorValues = BeginBitmapEdit(bitmap, TileSize, out _, out BitmapData bitmapData);

        for (int x = 0; x < TileSize.X; x++)
        {
            for (int y = 0; y < TileSize.Y; y++)
            {
                int pixelIndex = y * TileSize.X + x;
                pixels[pixelIndex] = colorValues[pixelIndex * 4 + 3] != 0;
            }
        }

        bitmap.UnlockBits(bitmapData);
        return tile;
    }

    public TileSet(int angleCount = 0, int tileWidth = 16, int tileHeight = 16)
    {
        TileSize = new Vector2<int>(tileWidth, tileHeight);

        Tiles = new List<Tile>(angleCount);

        for (int i = 0; i < angleCount; i++)
        {
            Tiles.Add(new Tile(TileSize));
        }
    }

    public void Save(string path, int columnCount, Color[] groupColor, int[] groupOffset, Vector2<int> separation, Vector2<int> offset)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var cell = new Vector2<int>(TileSize.X + separation.X, TileSize.Y + separation.Y);
        int rowCount = (Tiles.Count & -columnCount) / columnCount;

        var tileMapSize = new Vector2<int>(
            offset.X  + columnCount * cell.X - separation.X, 
            offset.Y + rowCount     * cell.Y - separation.Y);

        Bitmap tileMap = DrawTileMap(columnCount, groupColor, groupOffset, tileMapSize, separation, offset);

        tileMap.Save(path, ImageFormat.Png);
    }

    public void SaveCollisionMap(string path, List<Tile> tiles, bool isWidths)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var writer = new BinaryWriter(File.Open(path, FileMode.CreateNew));
        {
            foreach (Tile tile in Tiles)
            {
                foreach (byte value in isWidths ? tile.Widths : tile.Heights)
                {
                    writer.Write(value);
                }
            }
        }
    }

    public Bitmap DrawTileMap(int columnCount, Color[] groupColor, 
        int[] groupOffset, Vector2<int> tileMapSize, Vector2<int> separation, Vector2<int> offset)
    {
        int groupCount = groupColor.Length;
        var tileMap = new Bitmap(tileMapSize.X, tileMapSize.Y);

        using (var graphics = Graphics.FromImage(tileMap))
        {
            Vector2<int> position = new();
            var tileBorder = new Rectangle(0, 0, TileSize.X, TileSize.Y);
            Bitmap groupSeparator = GetGroupSeparator();

            for (int group = 0; group < groupCount; group++)
            {
                foreach (Tile tile in Tiles)
                {
                    DrawTile(graphics, tile, separation, offset, tileBorder, columnCount, ref position);
                }

                while (groupOffset[group]-- > 0)
                {
                    DrawTile(graphics, groupSeparator, separation, offset, tileBorder, columnCount, ref position);
                }
            }
        }
        return tileMap;
    }

    private void DrawTile(Graphics graphics, Bitmap tile, Vector2<int> separation, Vector2<int> offset,
        Rectangle TileBorder, int columnCount, ref Vector2<int> position)
    {
        graphics.DrawImage(
            tile,
            new Rectangle(
                offset.X + position.X * (TileSize.X + separation.X),
                offset.Y + position.Y * (TileSize.Y + separation.Y),
                TileSize.X, TileSize.Y),
            TileBorder,
            GraphicsUnit.Pixel);

        if (++position.X >= columnCount)
        {
            position.X = 0;
            position.Y++;
        }
    }

    private Bitmap GetGroupSeparator()
    {
        var groupSeparator = new Bitmap(TileSize.X, TileSize.Y);
        byte[] colorValues = BeginBitmapEdit(groupSeparator, TileSize, out nint ptr, out BitmapData bitmapData);

        for (int i = TileSize.X * TileSize.Y; i > 0; i--)
        {
            byte whiteOrBlack = (byte)(i % 2 == 0 ? 255 : 0);
            for (int j = 1; j < 4; j++)
            {
                colorValues[i * 4 + j] = whiteOrBlack;
            }
            colorValues[i * 4] = 255;
        }

        EndBitmapEdit(groupSeparator, colorValues, ptr, bitmapData);
        return groupSeparator;
    }

    public void TileChangeLine(int tileIndex, Vector2<int> tilePosition, bool isLeftButtonPressed)
    {
        bool[] pixels = Tiles[tileIndex].Pixels;

        if (isLeftButtonPressed)
        {
            if (tilePosition.Y == 0 || !pixels[GetPixelIndex(tilePosition.X, tilePosition.Y)]||
                pixels[GetPixelIndex(tilePosition.X, tilePosition.Y - 1)])
            {
                for (int y = 0; y < TileSize.Y; y++)
                {
                    pixels[GetPixelIndex(tilePosition.X, y)] = y >= tilePosition.Y;
                }
            }
            else
            {
                for (int y = 0; y < TileSize.Y; y++)
                {
                    pixels[GetPixelIndex(tilePosition.X, y)] = false;
                }
            }
        }
        else
        {
            int pixelOnPositionIndex = GetPixelIndex(tilePosition.X, tilePosition.Y);
            pixels[pixelOnPositionIndex] = !pixels[pixelOnPositionIndex];
        }

        Tiles[tileIndex].Pixels = pixels;
    }

    private byte[] BeginBitmapEdit(WriteableBitmap bitmap, Vector2<int> areaSize, out nint ptr, out BitmapData bitmapData)
    {
        using (var frameBuffer = bitmap.Lock())
        {
            int length = TileSize.X * TileSize.Y * 4;
            var data = new byte[length];
            Marshal.Copy(frameBuffer.Address, data, 0, length);
        }
        bitmapData = bitmap.Lock(
            new Rectangle(0, 0, areaSize.X, areaSize.Y),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        ptr = bitmapData.Scan0;
        int bytes = Math.Abs(bitmapData.Stride) * areaSize.Y;
        var colorValues = new byte[bytes];

        Marshal.Copy(ptr, colorValues, 0, bytes);

        return colorValues;
    }

    private void EndBitmapEdit(Bitmap bitmap, byte[] colorValues, nint ptr, BitmapData bitmapData)
    {
        Marshal.Copy(colorValues, 0, ptr, colorValues.Length);
        bitmap.UnlockBits(bitmapData);
    }

    private int GetPixelIndex(int positionX, int positionY)
    {
        return positionY * TileSize.X + positionX;
    }

    public void InsertTile(int tileIndex)
    {
        Tiles.Insert(tileIndex, new Tile(TileSize));
    }

    public void RemoveTile(int tileIndex)
    {
        Tiles.RemoveAt(tileIndex);
    }
}
