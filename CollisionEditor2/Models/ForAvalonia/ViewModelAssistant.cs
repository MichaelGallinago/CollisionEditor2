using System.Drawing.Imaging;
using System.Drawing;

namespace CollisionEditor2.Models.ForAvalonia;

public class ViewModelAssistant
{
    private const int dpi = 96;

    public static void SupplementElements(AngleMap angleMap, TileSet tileSet)
    {
        if (tileSet.Tiles.Count < angleMap.Values.Count)
        {
            Size size = tileSet.TileSize;
            for (int i = tileSet.Tiles.Count; i < angleMap.Values.Count; i++)
            {
                tileSet.Tiles.Add(new Bitmap(size.Width, size.Height));
                tileSet.WidthMap.Add(new byte[size.Width]);
                tileSet.HeightMap.Add(new byte[size.Height]);
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

    public static Avalonia.Media.Imaging.Bitmap BitmapConvert(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        var avaloniaBitmap = new Avalonia.Media.Imaging.Bitmap(
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul,
            bitmapData.Scan0,
            new Avalonia.PixelSize(bitmapData.Width, bitmapData.Height),
            new Avalonia.Vector(dpi, dpi),
            bitmapData.Stride);

        bitmap.UnlockBits(bitmapData);

        return avaloniaBitmap;
    }

    public static Avalonia.Media.Imaging.Bitmap GetBitmap(string path, out Size size)
    {
        var Bitmap = new Bitmap(path);
        size = new Size(Bitmap.Width, Bitmap.Height);
        return BitmapConvert(new Bitmap(path));
    }
}
