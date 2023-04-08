using System.Globalization;
using System;

namespace CollisionEditor2.Models.ForAvalonia;

public static class AngleService
{
    public const int HexAngleMaxLength = 2;
    public const int HexAnglePrefixLength = 2;

    private const double convertByteToFull = 1.40625d;

    public static Angles GetAngles(AngleMap angleMap, int chosenTile)
    {
        byte angle = angleMap.Values[chosenTile];

        return new Angles(angle, GetHexAngle(angle), GetFullAngle(angle));
    }

    public static string GetHexAngle(byte angle)
    {
        return "0x" + string.Format("{0:X}", angle).PadLeft(HexAngleMaxLength, '0');
    }

    public static double GetFullAngle(byte angle)
    {
        return Math.Round((byte.MaxValue + 1 - angle) * convertByteToFull, 1);
    }

    public static byte GetByteAngle(string hexAngle)
    {
        return byte.Parse(hexAngle[HexAnglePrefixLength..], NumberStyles.HexNumber);
    }
}
