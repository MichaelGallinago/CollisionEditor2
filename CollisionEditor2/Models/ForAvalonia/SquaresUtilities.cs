using CollisionEditor2.Views;
using Avalonia.Controls;
using Avalonia;

namespace CollisionEditor2.Models.ForAvalonia;

public static class SquaresUtilities
{
    public static void MoveSquare(MainWindow mainWindow, PixelPoint position,
        SquareAndPosition firstSquare, SquareAndPosition secondSquare)
    {
        firstSquare.Square.SetValue(Grid.ColumnProperty, position.X);
        firstSquare.Square.SetValue(Grid.RowProperty, position.Y);

        bool isFirstExists = mainWindow.RectanglesGrid.Children.Contains(firstSquare.Square);

        if (isFirstExists && Equals(position, firstSquare.Position))
        {
            mainWindow.RectanglesGrid.Children.Remove(firstSquare.Square);
            return;
        }

        firstSquare.Position = position;

        if (Equals(position, secondSquare.Position) && mainWindow.RectanglesGrid.Children.Contains(secondSquare.Square))
        {
            mainWindow.RectanglesGrid.Children.Remove(secondSquare.Square);
        }

        if (!isFirstExists)
        {
            mainWindow.RectanglesGrid.Children.Add(firstSquare.Square);
        }
    }
}
