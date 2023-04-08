using System.Text;

namespace CollisionEditor2.Models.ForAvalonia
{
    public class TileService
    {
        public static string GetCollisionValues(byte[] collisionArray)
        {
            StringBuilder builder = new();
            foreach (byte value in collisionArray)
            {
                //builder.Append((char)(value + (value < 10 ? 48 : 55)));
                builder.Append((char)(value + (value < 10 ? '0' : 'A')));
            }

            return string.Join(" ", builder.ToString().ToCharArray());
        }
    }
}
