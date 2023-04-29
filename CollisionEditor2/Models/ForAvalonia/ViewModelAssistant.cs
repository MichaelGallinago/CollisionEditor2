using System.Runtime.InteropServices;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

    public static Bitmap GetBitmapFromPixelArray(byte[] pixelColors, PixelSize bitmapSize)
    {
        var bitmap = new WriteableBitmap(
            bitmapSize, new Vector(dpi, dpi),
            PixelFormat.Rgba8888, AlphaFormat.Premul);

        using (var frameBuffer = bitmap.Lock())
        {
            Marshal.Copy(pixelColors, 0, frameBuffer.Address, pixelColors.Length);
        }

        return bitmap;
    }

    public static byte[] SKBitmapToPixelArray(SKBitmap tileMap)
    {
        return tileMap.GetPixelSpan().ToArray();
    }

    public static byte[] TileToPixelArray(Tile tile, OurColor color)
    {
        int channelsAmount = color.Channels.Length;
        var tileColors = new List<byte>(tile.Pixels.Length * channelsAmount);

        int alphaIndex = channelsAmount - 1;

        foreach (bool pixel in tile.Pixels)
        {
            for (var i = 0; i < alphaIndex; i++)
            {
                tileColors.Add(color.Channels[i]);
            }
            tileColors.Add((byte)(pixel ? color.Channels[alphaIndex] : 0));
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
