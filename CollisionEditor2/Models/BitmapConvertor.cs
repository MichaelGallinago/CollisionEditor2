using System.Runtime.InteropServices;
using System;
using SkiaSharp;
using Avalonia;

namespace CollisionEditor2.Models;

internal class BitmapConvertor
{
    public static byte[,,] SKBitmapToBitmapArray(SKBitmap bitmap)
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
}
