using System.Runtime.InteropServices;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia;

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

    public static Bitmap GetBitmapFromTile(Tile tile, OurColor color)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(tile.Heights.Length, tile.Widths.Length), 
            new Vector(dpi, dpi), 
            Avalonia.Platform.PixelFormat.Bgra8888, 
            Avalonia.Platform.AlphaFormat.Premul);

        var tileColors = new List<byte>(tile.Pixels.Length * 4);

        foreach (bool pixel in tile.Pixels)
        {
            tileColors.Add(color.Channels[2]);
            tileColors.Add(color.Channels[1]);
            tileColors.Add(color.Channels[0]);
            tileColors.Add((byte)(pixel ? color.Channels[3] : 0));
        }

        using (var frameBuffer = bitmap.Lock())
        {
            Marshal.Copy(tileColors.ToArray(), 0, frameBuffer.Address, tileColors.Count);
        }

        return bitmap;
    }

    public static Bitmap GetBitmap(string path, out PixelSize size)
    {
        var Bitmap = new Bitmap(path);
        size = new PixelSize((int)Bitmap.Size.Width, (int)Bitmap.Size.Height);
        return Bitmap;
    }
}
