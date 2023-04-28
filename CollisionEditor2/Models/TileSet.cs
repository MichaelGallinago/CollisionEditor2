using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Drawing;

namespace CollisionEditor2.Models;

public class TileSet
{
    public readonly Vector2<int> TileSize;

    public List<Tile> Tiles { get; private set; }

    public const int dpi = 96;

    public TileSet(string path, int tileWidth = 16, int tileHeight = 16,
        Vector2<int> separate = new(), Vector2<int> offset = new())
    {
        TileSize = new Vector2<int>(tileWidth, tileHeight);

        Tiles = new List<Tile>();

        SKImage image = SKImage.FromEncodedData(path);
        SKBitmap tileMap = RecolorTileMap(SKBitmap.FromImage(image));

        CreateTiles(tileMap, separate, offset);
    }

    private SKBitmap RecolorTileMap(SKBitmap tileMap)
    {
        byte[,,] pixelArray = SKBitmapToArray(tileMap);

        for (int x = pixelArray.GetLength(1) - 1; x >= 0; x--)
        {
            for (int y = pixelArray.GetLength(0) - 1; y >= 0; y--)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i % 4 == 3)
                    {
                        if (pixelArray[x, y, 3] > 0)
                        {
                            pixelArray[x, y, 3] = 255;
                        }
                        continue;
                    }
                    pixelArray[x, y, 3] = 0;
                }
            }
        }

        return ArrayToSKBitmap(pixelArray);
    }

    private void CreateTiles(SKBitmap tileMap, Vector2<int> separate, Vector2<int> offset)
    {
        var cellCount = new Vector2<int>(
            (tileMap.Width  - offset.X) / (TileSize.X + separate.X),
            (tileMap.Height - offset.Y) / (TileSize.Y + separate.Y));

        byte[,,] pixelArray = SKBitmapToArray(tileMap);

        var tile = new Tile(TileSize);
        bool[] tilePixels = tile.Pixels;

        for (int y = 0; y < cellCount.Y; y++)
        {
            for (int x = 0; x < cellCount.X; x++)
            {
                var tilePosition = new Vector2<int>(
                    x * (TileSize.X + separate.X) + offset.X,
                    y * (TileSize.Y + separate.Y) + offset.Y);
                
                for (int w = 0; w < TileSize.Y; w++)
                {
                    for (int z = 0; z < TileSize.X; z++)
                    {
                        tilePixels[w * TileSize.X + y] = pixelArray[
                            tilePosition.X + z, tilePosition.Y + w, 3] != 0;
                    }
                }
                tile.Pixels = tilePixels;
                Tiles.Add(tile);
            }
        }
    }

    private Tile CreateTileFromBitmap(SKBitmap bitmap)
    {
        var tile = new Tile(TileSize);
        bool[] tilePixels = tile.Pixels;

        byte[,,] pixelArray = SKBitmapToArray(bitmap);

        for (int x = 0; x < TileSize.X; x++)
        {
            for (int y = 0; y < TileSize.Y; y++)
            {
                tilePixels[y * TileSize.X + x] = pixelArray[x, y, 3] != 0;
            }
        }

        tile.Pixels = tilePixels;
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

    public void Save(string path, int columnCount, OurColor[] groupColor, int[] groupOffset, Vector2<int> separation, Vector2<int> offset)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var cell = new Vector2<int>(TileSize.X + separation.X, TileSize.Y + separation.Y);
        int rowCount = (Tiles.Count  & -columnCount) / columnCount;

        var tileMapSize = new Vector2<int>(
            offset.X + columnCount * cell.X - separation.X, 
            offset.Y + rowCount    * cell.Y - separation.Y);

        SKBitmap tileMap = DrawTileMap(columnCount, groupColor, groupOffset, tileMapSize, separation, offset);

        using var image = SKImage.FromBitmap(tileMap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    public void SaveCollisionMap(string path, List<Tile> tiles, bool isWidths)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var writer = new BinaryWriter(File.Open(path, FileMode.CreateNew));
        {
            foreach (Tile tile in tiles)
            {
                foreach (byte value in isWidths ? tile.Widths : tile.Heights)
                {
                    writer.Write(value);
                }
            }
        }
    }

    public Bitmap DrawTileMap(int columnCount, OurColor[] groupColor, 
        int[] groupOffset, Vector2<int> tileMapSize, Vector2<int> separation, Vector2<int> offset)
    {
        int groupCount = groupColor.Length;
        var tileMap = new SKBitmap(tileMapSize.X, tileMapSize.Y, 
            SKColorType.Rgba8888, SKAlphaType.Premul);

        byte[,,] pixelArray = SKBitmapToArray(tileMap);

        Vector2<int> position = new();
        SKBitmap groupSeparator = GetGroupSeparator();

        for (int group = 0; group < groupCount; group++)
        {
            foreach (Tile tile in Tiles)
            {
                DrawTile(pixelArray, tile.Pixels, groupColor[group], separation, offset, columnCount, ref position);
            }

            while (groupOffset[group]-- > 0)
            {
                DrawTile(pixelArray, groupSeparator, groupColor[group], separation, offset, columnCount, ref position);
            }
        }
        return tileMap;
    }

    private void DrawTile(byte[,,] pixelArray, bool[] pixels, OurColor color, Vector2<int> separation,
        Vector2<int> offset, int columnCount, ref Vector2<int> position)
    {
        var tilePosition = new Vector2<int>(
            offset.X + position.X * (TileSize.X + separation.X),
            offset.Y + position.Y * (TileSize.Y + separation.Y));

        for (int y = 0; y < TileSize.Y; y++)
        {
            for (int x = 0; x < TileSize.X; x++)
            {
                
            }
        }

        if (++position.X >= columnCount)
        {
            position.X = 0;
            position.Y++;
        }
    }

    private SKBitmap GetGroupSeparator()
    {
        var groupSeparator = new SKBitmap(TileSize.X, TileSize.Y);

        byte[,,] pixelArray = SKBitmapToArray(groupSeparator);

        for (int y = 0; y < TileSize.Y; y++)
        {
            for (int x = 0; x < TileSize.X; x++)
            {
                byte whiteOrBlack = (byte)((y * TileSize.X + x) % 2 == 0 ? 255 : 0);
                for (int j = 0; j < 3; j++)
                {
                    pixelArray[x, y, j] = whiteOrBlack;
                }
                pixelArray[x, y, 3] = 255;
            }
        }

        ArrayToSKBitmap(pixelArray);
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

    private byte[,,] SKBitmapToArray(SKBitmap bitmap)
    {
        ReadOnlySpan<byte> span = bitmap.GetPixelSpan();

        byte[,,] pixelValues = new byte[bitmap.Height, bitmap.Width, 4];
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                int offset = (y * bitmap.Width + x) * bitmap.BytesPerPixel;
                for (int i = 0; i < 4; i++)
                {
                    pixelValues[y, x, i] = span[offset + 3 - i];
                }
            }
        }

        return pixelValues;
    }
    
    public static SKBitmap ArrayToSKBitmap(byte[,,] pixelArray)
    {
        int width = pixelArray.GetLength(1);
        int height = pixelArray.GetLength(0);

        uint[] pixelValues = new uint[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte red   = pixelArray[y, x, 0];
                byte green = pixelArray[y, x, 1];
                byte blue  = pixelArray[y, x, 2];
                byte alpha = pixelArray[y, x, 3];
                uint pixelValue = red + (uint)(green << 8) + (uint)(blue << 16) + (uint)(alpha << 24);
                pixelValues[y * width + x] = pixelValue;
            }
        }

        SKBitmap bitmap = new();
        GCHandle gcHandle = GCHandle.Alloc(pixelValues, GCHandleType.Pinned);
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        IntPtr ptr = gcHandle.AddrOfPinnedObject();
        int rowBytes = info.RowBytes;
        bitmap.InstallPixels(info, ptr, rowBytes, delegate { gcHandle.Free(); });

        return bitmap;
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
