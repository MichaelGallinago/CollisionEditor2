using System.Runtime.InteropServices;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia;
using SkiaSharp;

namespace CollisionEditor2.Models.ForAvalonia;

public class ViewModelAssistant
{
    private const int dpi = 96;

    public static void SupplementElements(AngleMap angleMap, TileSet tileSet)
    {
        if (tileSet.Tiles.Count < angleMap.Values.Count)
        {
            PixelSize size = tileSet.TileSize;
            for (int i = tileSet.Tiles.Count; i < angleMap.Values.Count; i++)
            {
                tileSet.Tiles.Add(new Tile(size));
            }
        }
        else
        {
            for (int i = angleMap.Values.Count; i < tileSet.Tiles.Count; i++)
            {
                angleMap.Values.Add(0);
            }
        }
    }

    public static Bitmap GetBitmapFromArray(byte[] pixelColors, PixelSize bitmapSize, OurColor color)
    {
        var bitmap = new WriteableBitmap(
            bitmapSize,
            new Vector(dpi, dpi),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul);

        using (var frameBuffer = bitmap.Lock())
        {
            Marshal.Copy(pixelColors, 0, frameBuffer.Address, pixelColors.Length);
        }

        return bitmap;
    }

    public static byte[] SKBitmapToArray(SKBitmap tileMap)
    {
        return tileMap.GetPixelSpan().ToArray();
    }

    public static byte[] TileToArray(Tile tile, OurColor color)
    {
        var tileColors = new List<byte>(tile.Pixels.Length * 4);

        foreach (bool pixel in tile.Pixels)
        {
            for (int i = 3; i > 0; i--)
            {
                tileColors.Add(color.Channels[i]);
            }
            tileColors.Add((byte)(pixel ? color.Channels[0] : 0));
        }

        return tileColors.ToArray();
    }

    public static Bitmap GetBitmap(string path, out PixelSize size)
    {
        var Bitmap = new Bitmap(path);
        size = new PixelSize((int)Bitmap.Size.Width, (int)Bitmap.Size.Height);
        return Bitmap;
    }
}
