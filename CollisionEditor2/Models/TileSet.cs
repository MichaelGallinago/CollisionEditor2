using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System;
using SkiaSharp;
using Avalonia;
using System.Linq;

namespace CollisionEditor2.Models;

public class TileSet
{
    public readonly PixelSize TileSize;

    public List<Tile> Tiles { get; private set; }

    public TileSet(string path, int tileWidth = 16, int tileHeight = 16,
        PixelSize separation = new(), PixelSize offset = new())
    {
        TileSize = new PixelSize(tileWidth, tileHeight);

        Tiles = new List<Tile>();

        SKImage image = SKImage.FromEncodedData(path);

        CreateTiles(SKBitmap.FromImage(image), separation, offset);
    }

    private void CreateTiles(SKBitmap tileMap, PixelSize separation, PixelSize offset)
    {
        var cellCount = new PixelPoint(
            (tileMap.Width  - offset.Width) / (TileSize.Width + separation.Width),
            (tileMap.Height - offset.Height) / (TileSize.Height + separation.Height));

        byte[,,] bitmapArray = SKBitmapToBitmapArray(tileMap);

        for (int y = 0; y < cellCount.Y; y++)
        {
            for (int x = 0; x < cellCount.X; x++)
            {
                var tilePosition = new PixelPoint(
                    x * (TileSize.Width + separation.Width) + offset.Width,
                    y * (TileSize.Height + separation.Height) + offset.Height);

                var tile = new Tile(TileSize);
                bool[] tilePixels = tile.Pixels;

                for (int w = 0; w < TileSize.Height; w++)
                {
                    for (int z = 0; z < TileSize.Width; z++)
                    {
                        tilePixels[w * TileSize.Width + z] = bitmapArray[
                            tilePosition.X + z, tilePosition.Y + w, 0] != 0;
                    }
                }

                tile.Pixels = tilePixels;
                Tiles.Add(tile);
            }
        }
    }

    public TileSet(int angleCount = 0, int tileWidth = 16, int tileHeight = 16)
    {
        TileSize = new PixelSize(tileWidth, tileHeight);

        Tiles = new List<Tile>(angleCount);

        for (int i = 0; i < angleCount; i++)
        {
            Tiles.Add(new Tile(TileSize));
        }
    }

    public void SaveTileMap(string path, SKBitmap tileMap)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

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

    public SKBitmap DrawTileMap(int columnCount, OurColor[] groupColor, 
        int[] groupOffset, PixelSize separation, PixelSize offset)
    {
        var cell = new PixelSize(
            TileSize.Width + separation.Width, 
            TileSize.Height + separation.Height);

        int rowCount = ((Tiles.Count * groupOffset.Length 
            + groupOffset.Sum()) & -columnCount) / columnCount;

        var tileMapSize = new PixelSize(
            offset.Width + columnCount * cell.Width - separation.Width,
            offset.Height + rowCount * cell.Height - separation.Height);

        var tileMap = new SKBitmap(
            tileMapSize.Width, tileMapSize.Height, 
            SKColorType.Rgba8888, SKAlphaType.Premul);
        byte[,,] bitmapArray = SKBitmapToBitmapArray(tileMap);

        int groupCount = groupColor.Length;
        PixelPoint position = new();

        var white = new OurColor(255, 255, 255, 255);

        for (int group = 0; group < groupCount; group++)
        {
            foreach (Tile tile in Tiles)
            {
                DrawTile(ref bitmapArray, tile.Pixels, groupColor[group], 
                    separation, offset, columnCount, ref position);
            }

            while (groupOffset[group]-- > 0)
            {
                DrawTile(ref bitmapArray, null, white,
                    separation, offset, columnCount, ref position);
            }
        }
        return BitmapArrayToSKBitmap(bitmapArray);
    }

    private void DrawTile(ref byte[,,] bitmapArray, bool[]? tilePixels, OurColor tileColor,
        PixelSize separation, PixelSize offset, int columnCount, ref PixelPoint position)
    {
        OurColor secondColor;
        if (tilePixels is null)
        {
            tilePixels = new bool[TileSize.Width * TileSize.Height];
            for (int i = 0; i < tilePixels.Length; i += 2)
            {
                tilePixels[i] = true;
            }
            secondColor = new OurColor(0, 0, 0, 255);
        }
        else
        {
            secondColor = new OurColor(0, 0, 0, 0);
        }

        var tilePosition = new PixelPoint(
            offset.Width  + position.X * (TileSize.Width + separation.Width),
            offset.Height + position.Y * (TileSize.Height + separation.Height));

        for (int y = 0; y < TileSize.Height; y++)
        {
            for (int x = 0; x < TileSize.Width; x++)
            {
                OurColor pixelColor = tilePixels[y * TileSize.Width + x] ? tileColor : secondColor;
                for (int i = 0; i < 4; i++)
                {
                    bitmapArray[tilePosition.X + x, tilePosition.Y + y, i] = pixelColor.Channels[i];
                }
            }
        }

        position = position.X + 1 >= columnCount ? 
            new PixelPoint(0, position.Y + 1) :
            new PixelPoint(position.X + 1, position.Y);
    }

    public void ChangeTile(int tileIndex, PixelPoint pixelPosition, bool isLeftButtonPressed)
    {
        bool[] pixels = Tiles[tileIndex].Pixels;

        if (isLeftButtonPressed)
        {
            if (!pixels[GetPixelIndex(pixelPosition.X, pixelPosition.Y)] ||
                pixelPosition.Y != 0 && pixels[GetPixelIndex(pixelPosition.X, pixelPosition.Y - 1)])
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    pixels[GetPixelIndex(pixelPosition.X, y)] = y >= pixelPosition.Y;
                }
            }
            else
            {
                for (int y = 0; y < TileSize.Height; y++)
                {
                    pixels[GetPixelIndex(pixelPosition.X, y)] = false;
                }
            }
        }
        else
        {
            int pixelOnPositionIndex = GetPixelIndex(pixelPosition.X, pixelPosition.Y);
            pixels[pixelOnPositionIndex] = !pixels[pixelOnPositionIndex];
        }

        Tiles[tileIndex].Pixels = pixels;
    }

    private byte[,,] SKBitmapToBitmapArray(SKBitmap bitmap)
    {
        ReadOnlySpan<byte> span = bitmap.GetPixelSpan();

        byte[,,] pixelValues = new byte[bitmap.Width, bitmap.Height, 4];
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                int offset = (y * bitmap.Width + x) * bitmap.BytesPerPixel;
                for (int i = 0; i < 4; i++)
                {
                    pixelValues[x, y, i] = span[offset + 3 - i];
                }
            }
        }

        return pixelValues;
    }
    
    public static SKBitmap BitmapArrayToSKBitmap(byte[,,] bitmapArray)
    {
        var bitmapSize = new PixelSize(bitmapArray.GetLength(0), bitmapArray.GetLength(1));

        uint[] pixelValues = new uint[bitmapSize.Width * bitmapSize.Height];
        for (int y = 0; y < bitmapSize.Height; y++)
        {
            for (int x = 0; x < bitmapSize.Width; x++)
            {
                uint pixelValue = 0;
                for (int i = 0; i < 4; i++)
                {
                    pixelValue += (uint)(bitmapArray[x, y, i] << (8 * i));
                }
                pixelValues[y * bitmapSize.Width + x] = pixelValue;
            }
        }

        SKBitmap bitmap = new();
        GCHandle gcHandle = GCHandle.Alloc(pixelValues, GCHandleType.Pinned);
        var info = new SKImageInfo(bitmapSize.Width, bitmapSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

        IntPtr ptr = gcHandle.AddrOfPinnedObject();
        int rowBytes = info.RowBytes;
        bitmap.InstallPixels(info, ptr, rowBytes, delegate { gcHandle.Free(); });

        return bitmap;
    }

    private int GetPixelIndex(int positionX, int positionY)
    {
        return positionY * TileSize.Width + positionX;
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
