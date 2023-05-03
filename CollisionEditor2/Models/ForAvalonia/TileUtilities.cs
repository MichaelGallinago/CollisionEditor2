using System.Collections.Generic;
using System.Text;
using System.IO;
using SkiaSharp;

namespace CollisionEditor2.Models.ForAvalonia;

public static class TileUtilities
{
    private const byte digitsNumber = 10;

    public static string GetCollisionValues(byte[] collisionArray)
    {
        StringBuilder builder = new();
        foreach (byte value in collisionArray)
        {
            builder.Append((char)((value < digitsNumber ? '0' : 'A' - digitsNumber) + value));
        }

        return string.Join(" ", builder.ToString().ToCharArray());
    }

    public static void SaveTileMap(string path, SKBitmap tileMap)
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

    public static void SaveCollisionMap(string path, List<Tile> tiles, bool isWidths)
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
}
