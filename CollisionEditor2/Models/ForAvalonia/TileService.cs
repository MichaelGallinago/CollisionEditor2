using System.Text;

namespace CollisionEditor2.Models.ForAvalonia;

public class TileService
{
    private const byte digitsNumber = 10;

    public static string GetCollisionValues(byte[] collisionArray)
    {
        StringBuilder builder = new();
        foreach (byte value in collisionArray)
        {
            builder.Append((char)((value < digitsNumber ? '0' : 'A' - digitsNumber) + value));
        }

        return string.Join(" ", builder.ToString().ToCharArray());
    }
}
