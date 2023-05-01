using System.Collections.Generic;
using System.Linq;
using System;
using SkiaSharp;
using Avalonia;

namespace CollisionEditor2.Models;

public class TileSet
{
    public PixelSize TileSize { get; }

    public List<Tile> Tiles { get; private set; }

    public TileSet(string path, int tileWidth = 16, int tileHeight = 16,
        PixelSize separation = new(), PixelSize offset = new())
    {
        TileSize = new PixelSize(tileWidth, tileHeight);

        Tiles = new List<Tile>();

        SKImage image = SKImage.FromEncodedData(path);

        CreateTiles(SKBitmap.FromImage(image), separation, offset);
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

    public SKBitmap DrawTileMap(int columnCount, OurColor[] groupColor, 
        int[] groupOffset, PixelSize separation, PixelSize offset)
    {
        var cell = new PixelSize(
            TileSize.Width + separation.Width, 
            TileSize.Height + separation.Height);

        int rowCount = (int)Math.Ceiling((Tiles.Count * groupOffset.Length 
            + groupOffset.Sum()) / (double)columnCount);

        var tileMapSize = new PixelSize(
            offset.Width + columnCount * cell.Width - separation.Width,
            offset.Height + rowCount * cell.Height - separation.Height);

        var tileMap = new SKBitmap(
            tileMapSize.Width, tileMapSize.Height, 
            SKColorType.Rgba8888, SKAlphaType.Premul);
        byte[,,] bitmapArray = BitmapConvertor.GetBitmapArrayFromSKBitmap(tileMap);

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
        return BitmapConvertor.GetSKBitmapFromBitmapArray(bitmapArray);
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

    public void InsertTile(int tileIndex)
    {
        Tiles.Insert(tileIndex, new Tile(TileSize));
    }

    public void RemoveTile(int tileIndex)
    {
        Tiles.RemoveAt(tileIndex);
    }

    private void CreateTiles(SKBitmap tileMap, PixelSize separation, PixelSize offset)
    {
        var cellCount = new PixelPoint(
            (tileMap.Width - offset.Width) / (TileSize.Width + separation.Width),
            (tileMap.Height - offset.Height) / (TileSize.Height + separation.Height));

        byte[,,] bitmapArray = BitmapConvertor.GetBitmapArrayFromSKBitmap(tileMap);

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

    private void DrawTile(ref byte[,,] bitmapArray, bool[]? tilePixels, OurColor tileColor,
        PixelSize separation, PixelSize offset, int columnCount, ref PixelPoint position)
    {
        OurColor secondColor;
        if (tilePixels is null)
        {
            tilePixels = new bool[TileSize.Width * TileSize.Height];
            for (int y = 0; y < TileSize.Height; y++)
            {
                for (int x = 0; x < TileSize.Width; x++)
                {
                    tilePixels[y * TileSize.Width + x] = (x + y) % 4 < 2;
                }
            }
            secondColor = new OurColor(0, 0, 0, 255);
        }
        else
        {
            secondColor = new OurColor(0, 0, 0, 0);
        }

        var tilePosition = new PixelPoint(
            offset.Width + position.X * (TileSize.Width + separation.Width),
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

    private int GetPixelIndex(int positionX, int positionY)
    {
        return positionY * TileSize.Width + positionX;
    }
}
