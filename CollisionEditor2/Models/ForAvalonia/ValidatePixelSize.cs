namespace CollisionEditor2.Models.ForAvalonia
{
    public class ValidatedPixelSize
    {
        public int    Width        { get; set;}
        public int    Height       { get; set;}
        public string WidthString  { get; set;}
        public string HeightString { get; set;}

        public ValidatedPixelSize(int width, int height)
        {
            WidthString = width.ToString();
            HeightString = height.ToString();
            Width = width;
            Height = height;
        }
    }
}
