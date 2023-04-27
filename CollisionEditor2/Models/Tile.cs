using System.Drawing;

namespace CollisionEditor2.Models
{
    public class Tile
    {
        public Bitmap Tiles { get; private set; }
        public byte[] WidthMap { get; private set; }
        public byte[] HeightMap { get; private set; }
    }
}
