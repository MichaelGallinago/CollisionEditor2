using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Avalonia;

namespace CollisionEditor2.Models;

public class AngleMap
{
    public List<byte> Values { get; private set; }

    private const double convertRadiansToByte = 128 / Math.PI;

    public AngleMap(string path)
    {
        var reader = new BinaryReader(File.Open(path, FileMode.Open));
        Values = reader.ReadBytes((int)Math.Min(int.MaxValue, reader.BaseStream.Length)).ToList();
    }

    public AngleMap(int tileCount = 0)
    {
        Values = new List<byte>(new byte[tileCount]);
    }

    public void Save(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using BinaryWriter writer = new(File.Open(path, FileMode.CreateNew));
        {
            foreach (byte value in Values)
            {
                writer.Write(value);
            }
        }
    }

    public byte SetAngleWithLine(int tileIndex, PixelPoint positionGreen, PixelPoint positionBlue)
    {
        return Values[tileIndex] = (byte)(Math.Atan2(
            positionBlue.Y - positionGreen.Y, 
            positionBlue.X - positionGreen.X) 
            * convertRadiansToByte);
    }

    public byte SetAngle(int tileIndex, byte value)
    {
        return Values[tileIndex] = value;
    }

    public byte ChangeAngle(int tileIndex, int value)
    {
        return Values[tileIndex] = (byte)(Values[tileIndex] + value);
    }

    public void InsertAngle(int tileIndex)
    {
        Values.Insert(tileIndex, 0);
    }

    public void RemoveAngle(int tileIndex)
    {
        Values.RemoveAt(tileIndex);
    }
}
