namespace CollisionEditor2.Models.ForAvalonia;

// Color in RGBA format
public struct OurColor
{
    public byte[] Channels { get; set; }

    public OurColor(byte red, byte green, byte blue, byte alpha)
    {
        Channels = new byte[] { red, green, blue, alpha };
    }
}
