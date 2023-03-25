using CollisionEditor2.Models;

namespace CollisionEditor2.ViewModels;

public static class ViewModelAngleService
{
    public static Angles GetAngles(byte byteAngle)
    {
        return new Angles(byteAngle, ViewModelAssistant.GetHexAngle(byteAngle), ViewModelAssistant.GetFullAngle(byteAngle));
    }

    public static Angles GetAngles(string hexAngle)
    {
        var byteAngle = ViewModelAssistant.GetByteAngle(hexAngle);
        return new Angles(byteAngle, hexAngle, ViewModelAssistant.GetFullAngle(byteAngle));
    }
}
