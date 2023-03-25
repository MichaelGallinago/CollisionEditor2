﻿using CollisionEditor2.Models;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace CollisionEditor2.ViewServices;

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
