using CollisionEditor2.Models.ForAvalonia;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia;

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

        Bitmap tileMap = RecolorTileMap(new Bitmap(path));

        CreateTiles(tileMap, separate, offset);
    }

    private Bitmap RecolorTileMap(Bitmap tileMap)
    {
        byte[] colorValues = BeginBitmapEdit((WriteableBitmap)tileMap);

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

        EndBitmapEdit((WriteableBitmap)tileMap, colorValues);

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
                var tileBounds = new PixelRect(
                    x * (TileSize.X  + separate.X)  + offset.X,
                    y * (TileSize.Y + separate.Y) + offset.Y,
                    TileSize.X, TileSize.Y);
                Bitmap bitmap = (Bitmap)(IImage)new CroppedBitmap(tileMap, tileBounds);
                Tiles.Add(CreateTileFromBitmap(bitmap));
            }
        }
    }

    private Tile CreateTileFromBitmap(Bitmap bitmap)
    {
        var tile = new Tile(TileSize);
        bool[] pixels = tile.Pixels;
        byte[] colorValues = BeginBitmapEdit((WriteableBitmap)bitmap);

        for (int x = 0; x < TileSize.X; x++)
        {
            for (int y = 0; y < TileSize.Y; y++)
            {
                int pixelIndex = y * TileSize.X + x;
                pixels[pixelIndex] = colorValues[pixelIndex * 4 + 3] != 0;
            }
        }

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

        Bitmap tileMap = DrawTileMap(columnCount, groupColor, groupOffset, tileMapSize, separation, offset);

        tileMap.Save(path);
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
        var tileMap = new WriteableBitmap(
            new PixelSize(tileMapSize.X, tileMapSize.Y),
            new Vector(dpi, dpi),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        byte[] colorValues = BeginBitmapEdit(tileMap);

        Vector2<int> position = new();
        Bitmap groupSeparator = GetGroupSeparator();

        for (int group = 0; group < groupCount; group++)
        {
            foreach (Tile tile in Tiles)
            {
                DrawTile(colorValues, tile.Pixels, groupColor[group], separation, offset, columnCount, ref position);
            }

            while (groupOffset[group]-- > 0)
            {
                //DrawTile(colorValues, groupSeparator, groupColor[group], separation, offset, columnCount, ref position);
            }
        }
        return tileMap;
    }

    private void DrawTile(byte[] colorValues, bool[] pixels, OurColor color, Vector2<int> separation,
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

    private Bitmap GetGroupSeparator()
    {
        var groupSeparator = new WriteableBitmap(
            new PixelSize(TileSize.X, TileSize.Y),
            new Vector(dpi, dpi),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        byte[] colorValues = BeginBitmapEdit(groupSeparator);

        for (int i = TileSize.X * TileSize.Y; i > 0; i--)
        {
            byte whiteOrBlack = (byte)(i % 2 == 0 ? 255 : 0);
            for (int j = 1; j < 4; j++)
            {
                colorValues[i * 4 + j] = whiteOrBlack;
            }
            colorValues[i * 4] = 255;
        }

        EndBitmapEdit(groupSeparator, colorValues);
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

    private byte[] BeginBitmapEdit(WriteableBitmap bitmap)
    {
        int length = TileSize.X * TileSize.Y * 4;
        var colorValues = new byte[length];

        using (var frameBuffer = bitmap.Lock())
        {
            Marshal.Copy(frameBuffer.Address, colorValues, 0, length);
        }

        return colorValues;
    }

    private void EndBitmapEdit(WriteableBitmap bitmap, byte[] colorValues)
    {
        using var frameBuffer = bitmap.Lock();
        Marshal.Copy(colorValues, 0, frameBuffer.Address, colorValues.Length);
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
