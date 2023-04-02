using CollisionEditor2.Views;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using static System.Math;

namespace CollisionEditor2.ViewServices;

public static class RedLineService
{
    public static void DrawRedLine(MainWindow mainWindow, ref Line redLine)
    {
        var floatAngle = float.Parse(mainWindow.TextBlockFullAngle.Text.TrimEnd('°'));

        int size = (int)mainWindow.canvasForLine.Width / 2;

        double DegreeToRadian = PI / 180;

        double length = size / Cos(((floatAngle + 45) % 90 - 45) * DegreeToRadian);
        floatAngle += 90;

        var lineEdgePosition = new Point(
            length * Sin(floatAngle * DegreeToRadian), 
            length * Cos(floatAngle * DegreeToRadian));

        redLine = new()
        {
            StartPoint =  lineEdgePosition,
            EndPoint   = -lineEdgePosition,
        };

        Canvas.SetTop(redLine,  size);
        Canvas.SetLeft(redLine, size);

        redLine.Stroke = new SolidColorBrush(Colors.Red);

        mainWindow.canvasForLine.Children.Clear();
        mainWindow.canvasForLine.Children.Add(redLine);
    }
}
