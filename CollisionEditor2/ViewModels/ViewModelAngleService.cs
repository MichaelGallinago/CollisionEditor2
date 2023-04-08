using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.Models;

namespace CollisionEditor2.ViewModels;

public static class ViewModelAngleService
{
    public static Angles GetAngles(byte byteAngle)
    {
        return new Angles(byteAngle, AngleService.GetHexAngle(byteAngle), AngleService.GetFullAngle(byteAngle));
    }

    public static Angles GetAngles(string hexAngle)
    {
        var byteAngle = AngleService.GetByteAngle(hexAngle);

        return new Angles(byteAngle, hexAngle, AngleService.GetFullAngle(byteAngle));
    }
}
