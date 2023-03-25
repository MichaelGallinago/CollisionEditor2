namespace CollisionEditor2.Models;

public class Angles
{
    public byte   ByteAngle { get; private set; }
    public string HexAngle  { get; private set; }
    public double FullAngle { get; private set; }

    public Angles(byte byteAngle, string hexAngle, double fullAngle)
    {
        ByteAngle = byteAngle;
        HexAngle  = hexAngle;
        FullAngle = fullAngle;
    }
}
