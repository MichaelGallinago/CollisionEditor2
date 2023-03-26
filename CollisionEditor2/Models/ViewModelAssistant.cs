﻿using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using System.Text;
using System;

namespace CollisionEditor2.Models;

internal static class ViewModelAssistant
{
    public static Angles GetAngles(AngleMap angleMap, uint chosenTile)
    {
        byte angle = angleMap.Values[(int)chosenTile];
        return new Angles(angle, GetHexAngle(angle), GetFullAngle(angle));
    }

    public static string GetCollisionValues(byte[] collisionArray)
    {
        var builder = new StringBuilder();
        foreach (byte value in collisionArray)
        {
            builder.Append((char)(value + (value < 10 ? 48 : 55)));
        }

        return string.Join(" ", builder.ToString().ToCharArray());
    }

    public static Avalonia.Media.Imaging.Bitmap BitmapConvert(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        var avaloniaBitmap = new Avalonia.Media.Imaging.Bitmap(
            Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul,
            bitmapData.Scan0,
            new Avalonia.PixelSize(bitmapData.Width, bitmapData.Height),
            new Avalonia.Vector(96, 96),
            bitmapData.Stride);

        bitmap.UnlockBits(bitmapData);

        return avaloniaBitmap;
    }

    public static string GetHexAngle(byte angle)
    {
        return "0x" + string.Format("{0:X}", angle).PadLeft(2, '0');
    }

    public static double GetFullAngle(byte angle)
    {
        return Math.Round((256 - angle) * 1.40625, 1);
    }

    public static byte GetByteAngle(string hexAngle)
    {
        return byte.Parse(hexAngle.Substring(2), NumberStyles.HexNumber);
    }

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
}
