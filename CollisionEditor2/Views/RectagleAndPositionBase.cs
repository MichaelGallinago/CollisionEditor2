using CollisionEditor.Model;
using System.Windows.Shapes;
using System.Windows.Media;

namespace CollisionEditor.View
{
    internal class SquareAndPosition
    {
        public Vector2<int> Position { get; set; } = new Vector2<int>();
        public Rectangle Square { get; set; } = new Rectangle();
        public Color Color { get; set; }

        public SquareAndPosition(Color color)
        {
            Square.Fill = new SolidColorBrush(color);
            Color = color;
        }
    }
}
