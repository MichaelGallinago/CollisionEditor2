using System.Globalization;
using System;

namespace CollisionEditor2.Models;

public class Angles
{
    public byte   ByteAngle { get; private set; }
    public string HexAngle  { get; private set; } = "0x00";
    public double FullAngle { get; private set; }

    public const int HexAngleMaxLength = 2;
    public const int HexAnglePrefixLength = 2;

    private const double convertByteToFull = 1.40625d;

    public static Angles FromHex(string hexAngle)
    {
        var angles = new Angles
        {
            ByteAngle = GetByteAngle(hexAngle),
            HexAngle  = hexAngle
        };
        angles.FullAngle = GetFullAngle(angles.ByteAngle);

        return angles;
    }

    public static Angles FromByte(byte byteAngle)
    {
        var angles = new Angles
        {
            ByteAngle = byteAngle,
            HexAngle  = GetHexAngle(byteAngle),
            FullAngle = GetFullAngle(byteAngle)
        };

        return angles;
    }

    public static string GetHexAngle(byte byteAngle)
    {
        return "0x" + string.Format("{0:X}", byteAngle).PadLeft(HexAngleMaxLength, '0');
    }

    public static double GetFullAngle(byte byteAngle)
    {
        return Math.Round((byte.MaxValue + 1 - byteAngle) * convertByteToFull, 1);
    }

    public static byte GetByteAngle(string hexAngle)
    {
        return byte.Parse(hexAngle[HexAnglePrefixLength..], NumberStyles.HexNumber);
    }
}
