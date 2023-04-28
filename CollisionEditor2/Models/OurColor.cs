namespace CollisionEditor2.Models
{
    public struct OurColor
    {
        public byte[] Channels { get; set; }

        public OurColor(byte[] channels)
        {
            Channels = channels;
        }

        public OurColor(byte red, byte green, byte blue, byte alpha)
        {
            Channels = new byte[] { red, green, blue, alpha };
        }
    }
}
