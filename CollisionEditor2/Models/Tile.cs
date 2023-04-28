using System;

namespace CollisionEditor2.Models
{
    public class Tile
    {
        public byte[] Widths { get; private set; }
        public byte[] Heights { get; private set; }
        public bool[] Pixels
        {
            get => pixels;
            set
            {
                if (value.Length != pixels.Length)
                {
                    throw new ArgumentException("Wrong array length");
                }

                pixels = value;

                for (int x = Heights.Length - 1; x >= 0; x--)
                {
                    for (int y = Widths.Length - 1; y >= 0; y--)
                    {
                        if (Pixels[y * Heights.Length + x])
                        {
                            Widths[y]++;
                            Heights[x]++;
                        }
                    }
                }
            }
        }

        private bool[] pixels;

        public Tile(Vector2<int> tileSize) 
        {
            Heights = new byte[tileSize.X];
            Widths  = new byte[tileSize.Y];
            pixels  = new bool[tileSize.Y * tileSize.X];
            Pixels  = pixels;
        }
    }
}
