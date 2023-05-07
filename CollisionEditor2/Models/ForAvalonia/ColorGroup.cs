using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionEditor2.Models.ForAvalonia
{
    public struct ColorGroup
    {
        
        public byte RedChannel    { get; set;} 
        public byte GreenChannel  { get; set; }
        public byte BlueChannel   { get; set; }
        public byte AlphaChannel  { get; set; }
        public int  OffsetInTiles { get; set; }
        public ColorGroup(byte redChannel, byte greenChannel, byte blueChannel, byte alphaChannel, int offsetInTiles) 
        {
            RedChannel = redChannel;
            GreenChannel = greenChannel;
            BlueChannel = blueChannel;
            AlphaChannel = alphaChannel;
            OffsetInTiles = offsetInTiles;
        }
    }
}
