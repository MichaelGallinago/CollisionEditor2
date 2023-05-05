using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using CollisionEditor2.Views;
using static System.Math;

namespace CollisionEditor2.ViewUtilities;

public static class RedLineUtilities
{
    public static void DrawRedLine(MainWindow mainWindow, ref Line redLine)
    {
        float floatAngle = float.Parse(mainWindow.TextBlockFullAngle.Text.TrimEnd('°'));

        int size = (int)mainWindow.canvasForLine.Width / 2;

        double degreeToRadian = PI / 180;

        double length = size / Cos(((floatAngle + 45) % 90 - 45) * degreeToRadian);
        floatAngle += 90;

        var lineEdgePosition = new Point(
            length * Sin(floatAngle * degreeToRadian),
            length * Cos(floatAngle * degreeToRadian));

        redLine = new()
        {
            StartPoint = lineEdgePosition,
            EndPoint = -lineEdgePosition,
        };

        Canvas.SetTop(redLine, size);
        Canvas.SetLeft(redLine, size);

        redLine.Stroke = new SolidColorBrush(Colors.Red);

        mainWindow.canvasForLine.Children.Clear();
        mainWindow.canvasForLine.Children.Add(redLine);
    }
}
