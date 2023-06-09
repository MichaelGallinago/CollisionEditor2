﻿using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace CollisionEditor2.Models.ForAvalonia;

public class SquareAndPosition
{
    public PixelPoint Position { get; set; } = new();
    public Rectangle Square { get; set; } = new();
    public Color Color { get; set; }

    public SquareAndPosition(Color color)
    {
        Square.Fill = new SolidColorBrush(color);
        Color = color;
    }
}
