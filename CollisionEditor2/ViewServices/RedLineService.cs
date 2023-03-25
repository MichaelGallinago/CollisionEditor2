using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using CollisionEditor2.Views;

namespace CollisionEditor2.ViewServices;

internal static class RedLineService
{

    public static void DrawRedLine(MainWindow mainWindow,ref Line redLine)
    {
        float floatAngle = float.Parse(mainWindow.TextBlockFullAngle.Text.TrimEnd('°'));

        if (floatAngle > 180f)
            floatAngle -= 180f;

        int size = (int)mainWindow.canvasForLine.Width / 2;

        Line newLine = new Line();
        double length = size / Math.Abs(Math.Cos((-45 + (floatAngle + 45) % 90) / 180 * Math.PI));
        floatAngle += 90;
        newLine.X1 = length * Math.Sin(floatAngle / 180 * Math.PI);
        newLine.Y1 = length * Math.Cos(floatAngle / 180 * Math.PI);
        newLine.X2 = -newLine.X1;
        newLine.Y2 = -newLine.Y1;
        Canvas.SetTop(newLine, size);
        Canvas.SetLeft(newLine, size);
        newLine.Stroke = new SolidColorBrush(Colors.Red);
        newLine.Fill = new SolidColorBrush(Colors.Red);

        mainWindow.canvasForLine.Children.Remove(redLine);
        redLine = newLine;
        mainWindow.canvasForLine.Children.Add(newLine);
    }
}
