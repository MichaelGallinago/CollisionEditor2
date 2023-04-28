using Avalonia.Media.Imaging;

namespace CollisionEditor2.Models.ForAvalonia;

public class ViewModelAssistant
{
    private const int dpi = 96;

    public static void SupplementElements(AngleMap angleMap, TileSet tileSet)
    {
        if (tileSet.Tiles.Count < angleMap.Values.Count)
        {
            Vector2<int> size = tileSet.TileSize;
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

    public static Bitmap BitmapConvert(Tile tile)
    {
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        var avaloniaBitmap = new Bitmap(
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul,
            bitmapData.Scan0,
            new Avalonia.PixelSize(tile.Heights.Length, tile.Widths.Length),
            new Avalonia.Vector(dpi, dpi),
            bitmapData.Stride);

        var avaloniaBitmap = new Bitmap();

        bitmap.UnlockBits(bitmapData);

        return avaloniaBitmap;
    }

    public static Bitmap GetBitmap(string path, out Vector2<int> size)
    {
        var Bitmap = new Bitmap(path);
        size = new Vector2<int>((int)Bitmap.Size.Width, (int)Bitmap.Size.Height);
        return Bitmap;
    }
}
