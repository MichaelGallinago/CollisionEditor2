using Avalonia;
using CollisionEditor2.Models.ForAvalonia;
using Microsoft.CodeAnalysis.Text;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

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

        DrawTiles(bitmapArray, groupOffset, groupColor, separation, 
            offset, columnCount, groupCount);

        return BitmapConvertor.GetSKBitmapFromBitmapArray(bitmapArray);
    }

    public void ChangeTile(int tileIndex, PixelPoint pixelPosition, bool isLeftButtonPressed)
    {
        bool[] pixels = Tiles[tileIndex].Pixels;

        if (isLeftButtonPressed)
        {
            ChangeHeight(pixels, pixelPosition);
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

    private void ChangeHeight(bool[] pixels, PixelPoint pixelPosition)
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

                CreateTileFromTileMap(bitmapArray, tilePosition);
            }
        }
    }

    private void CreateTileFromTileMap(byte[,,] bitmapArray, PixelPoint tilePosition)
    {
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

    private void DrawTiles(byte[,,] bitmapArray, int[] groupOffset, OurColor[] groupColor,
        PixelSize separation, PixelSize offset, int columnCount, int groupCount)
    {
        var white = new OurColor(255, 255, 255, 255);
        PixelPoint position = new();

        for (int group = 0; group < groupCount; group++)
        {
            foreach (Tile tile in Tiles)
            {
                DrawTile(bitmapArray, tile.Pixels, groupColor[group],
                    separation, offset, columnCount, ref position);
            }

            while (groupOffset[group]-- > 0)
            {
                DrawTile(bitmapArray, null, white,
                    separation, offset, columnCount, ref position);
            }
        }
    }

    private void DrawTile(byte[,,] bitmapArray, bool[]? tilePixels, OurColor tileColor,
        PixelSize separation, PixelSize offset, int columnCount, ref PixelPoint position)
    {
        OurColor secondColor = GetSecondColor(tilePixels, out bool[] checkedTilePixels);

        var tilePosition = new PixelPoint(
            offset.Width + position.X * (TileSize.Width + separation.Width),
            offset.Height + position.Y * (TileSize.Height + separation.Height));

        for (int y = 0; y < TileSize.Height; y++)
        {
            for (int x = 0; x < TileSize.Width; x++)
            {
                OurColor pixelColor = checkedTilePixels[y * TileSize.Width + x] ? tileColor : secondColor;
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

    private OurColor GetSecondColor(bool[]? tilePixels, out bool[] checkedTilePixels)
    {
        if (tilePixels is not null)
        {
            checkedTilePixels = tilePixels;
            return new OurColor(0, 0, 0, 0);
        }

        checkedTilePixels = new bool[TileSize.Width * TileSize.Height];
        for (int y = 0; y < TileSize.Height; y++)
        {
            for (int x = 0; x < TileSize.Width; x++)
            {
                checkedTilePixels[y * TileSize.Width + x] = (x + y) % 4 < 2;
            }
        }

        return new OurColor(0, 0, 0, 255);
    }

    private int GetPixelIndex(int positionX, int positionY)
    {
        return positionY * TileSize.Width + positionX;
    }
}
