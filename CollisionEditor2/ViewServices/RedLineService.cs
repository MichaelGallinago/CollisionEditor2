using CollisionEditor2.Views;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using System;

namespace CollisionEditor2.ViewServices;

internal static class RedLineService
{
    public static void DrawRedLine(MainWindow mainWindow, ref Line redLine)
    {
        float floatAngle = float.Parse(mainWindow.TextBlockFullAngle.Text.TrimEnd('°'));

        if (floatAngle > 180f)
        {
            floatAngle -= 180f;
        }

        int size = (int)mainWindow.canvasForLine.Width / 2;

        double length = size / Math.Abs(Math.Cos((-45 + (floatAngle + 45) % 90) / 180 * Math.PI));
        floatAngle += 90;

        var lineEdgePosition = new Point(
            length * Math.Sin(floatAngle / 180 * Math.PI), 
            length * Math.Cos(floatAngle / 180 * Math.PI));

        Line newLine = new()
        {
            StartPoint =  lineEdgePosition,
            EndPoint   = -lineEdgePosition,
        };

        Canvas.SetTop(newLine,  size);
        Canvas.SetLeft(newLine, size);

        newLine.Stroke = new SolidColorBrush(Colors.Red);
        newLine.Fill   = new SolidColorBrush(Colors.Red);

        mainWindow.canvasForLine.Children.Remove(redLine);
        redLine = newLine;
        mainWindow.canvasForLine.Children.Add(newLine);
    }
}
