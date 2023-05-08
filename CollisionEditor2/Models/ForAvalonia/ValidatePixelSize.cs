using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionEditor2.Models.ForAvalonia
{
    public class ValidatePixelSize
    {
        public int     Width        { get; set;}
        public int     Height       { get; set;}
        public string WidthString  { get; set;}
        public string HeightString { get; set;}

        public ValidatePixelSize(int width, int height)
        {
            WidthString = width.ToString();
            HeightString = height.ToString();
            this.Width = width;
            this.Height = height;
        }
    }
}
