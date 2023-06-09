using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.ViewModels;
using CollisionEditor2.ViewUtilities;
using System;
using System.Threading.Tasks;

namespace CollisionEditor2.Views;

public partial class MainWindow : Window
{
    public const int TileMapTileScale = 2;

    private const int tileMapSeparation = 4;
    private const int menuHeight = 20;
    private const int textAndButtonsHeight = 20;
    private const int upAndDownButtonsWidth = 23;
    private const int gridHeight = 128;
    private const int countHeightParts = 404;
    private const int countWidthParts = 587;
    private const int tileMapBorderWidthWithoutScrollBar = 300;
    private const int tileMapScrollBarWidth = 18;

    private bool isTileEditorMode = false;
    private bool isPointerInRectanglesGrid = false;
    private Line redLine = new();

    private (SquareAndPosition, SquareAndPosition) blueAndGreenSquare =
        (new SquareAndPosition(Color.FromRgb(0, 0, 255)),
        new SquareAndPosition(Color.FromRgb(0, 255, 0)));

    public int LastSelectedTile { get; set; }
    public MainViewModel WindowMain { get; }

    public MainWindow()
    {
        InitializeComponent();
        WindowMain = new MainViewModel(this);
        this.GetObservable(ClientSizeProperty).Subscribe(WindowSizeChanged);
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public void DrawRedLine()
    {
        if (WindowMain.AngleMap.Values.Count > 0)
        {
            RedLineUtilities.DrawRedLine(WindowMain.Window, ref redLine);
        }
    }

    public void WindowSizeChanged(Size size)
    {
        int countOfTiles = WindowMain.TileSet.Tiles.Count;
        PixelSize tileSize = WindowMain.TileSet.TileSize;

        double actualHeightTextAndButtons = (size.Height - menuHeight) / countHeightParts * textAndButtonsHeight;
        double actualWidthUpAndDownButtons = size.Width / countWidthParts * upAndDownButtonsWidth;
        double actualFontSize = Math.Min((25.4 / 96 * actualHeightTextAndButtons) / 0.35 - 4, (25.4 / 96 * (size.Width / countWidthParts * 43)) / 0.35 - 21);

        double actualHeightGrid = (size.Height - menuHeight) / countHeightParts * gridHeight;

        var TileGridSize = new Size(
            (int)actualHeightGrid / tileSize.Width * tileSize.Width,
            (int)actualHeightGrid / tileSize.Height * tileSize.Height);

        ScalingTileGrid(TileGridSize);
        ScalingModSwitchButton(actualHeightTextAndButtons, actualFontSize);
        ScalingHeightsAndWidths(actualHeightTextAndButtons, actualFontSize);
        ScalingAngleTextBoxesAndTextBlock(actualHeightTextAndButtons, actualFontSize);
        ScalingIncAndDecAngleButtons(actualHeightTextAndButtons, actualWidthUpAndDownButtons);

        int tileWidth = tileSize.Width * TileMapTileScale;
        int tileHeight = tileSize.Height * TileMapTileScale;
        ScalingTileMap(size, countOfTiles, tileWidth, tileHeight);
        ScalingRightPanelButtonsAndTextBox(actualHeightTextAndButtons, actualFontSize);

        DrawRedLine();
    }

    private void ScalingTileMap(Size size, int countOfTiles, int tileWidth, int tileHeight)
    {
        TileMapGrid.Width = tileMapBorderWidthWithoutScrollBar * (int)size.Width / (int)MinWidth / (tileWidth + tileMapSeparation) * (tileWidth + tileMapSeparation);
        TileMapGrid.Columns = ((int)TileMapGrid.Width + tileMapSeparation) / (tileWidth + tileMapSeparation);
        TileMapGrid.Height = (int)Math.Ceiling((double)countOfTiles / TileMapGrid.Columns) * (tileHeight + tileMapSeparation);
        TileMapBorder.Width = TileMapGrid.Width + tileMapScrollBarWidth;
    }

    private void ScalingTileGrid(Size TileGridSize)
    {
        TileGrid.Width = TileGridSize.Width;
        TileGrid.Height = TileGridSize.Height;

        RectanglesGrid.Width = TileGridSize.Width;
        RectanglesGrid.Height = TileGridSize.Height;

        canvasForLine.Width = TileGridSize.Width;
        canvasForLine.Height = TileGridSize.Height;
    }

    private void ScalingModSwitchButton(double actualHeightTextAndButtons, double actualFontSize)
    {
        ModSwitchButton.Height = actualHeightTextAndButtons + 5;
        ModSwitchButton.FontSize = actualFontSize;
    }

    private void ScalingHeightsAndWidths(double actualHeightTextAndButtons, double actualFontSize)
    {
        Heights.Height = actualHeightTextAndButtons;
        Heights.FontSize = actualFontSize;

        Widths.Height = actualHeightTextAndButtons;
        Widths.FontSize = actualFontSize;
    }

    private void ScalingAngleTextBoxesAndTextBlock(double actualHeightTextAndButtons, double actualFontSize)
    {
        TextBlockFullAngle.Height = actualHeightTextAndButtons - 2;
        TextBlockFullAngle.FontSize = actualFontSize;

        TextBoxByteAngle.Height = actualHeightTextAndButtons - 2;
        TextBoxByteAngle.FontSize = actualFontSize;

        TextBoxHexAngle.Height = actualHeightTextAndButtons - 2;
        TextBoxHexAngle.FontSize = actualFontSize;
    }

    private void ScalingRightPanelButtonsAndTextBox(double actualHeightTextAndButtons, double actualFontSize)
    {
        SelectTileTextBox.Height = actualHeightTextAndButtons - 2;
        SelectTileTextBox.FontSize = actualFontSize;
        SelectTileButton.Height = actualHeightTextAndButtons - 2;
        SelectTileButton.FontSize = actualFontSize;
        AddTileButton.Height = actualHeightTextAndButtons - 2;
        AddTileButton.FontSize = actualFontSize;
        DeleteTileButton.Height = actualHeightTextAndButtons - 2;
        DeleteTileButton.FontSize = actualFontSize;
    }

    private void ScalingIncAndDecAngleButtons(double actualHeightTextAndButtons, double actualWidthUpAndDownButtons)
    {
        ByteAngleIncrimentButton.Height = actualHeightTextAndButtons / 2;
        ByteAngleIncrimentButton.Width = actualWidthUpAndDownButtons;
        ByteAngleDecrementButton.Height = actualHeightTextAndButtons / 2;
        ByteAngleDecrementButton.Width = actualWidthUpAndDownButtons;

        TriangleUpByteAngle.Height = actualHeightTextAndButtons / 2 - 5;
        TriangleUpByteAngle.Width = actualWidthUpAndDownButtons / 2 - 5;
        TriangleDownByteAngle.Height = actualHeightTextAndButtons / 2 - 5;
        TriangleDownByteAngle.Width = actualWidthUpAndDownButtons / 2 - 5;

        HexAngleIncrimentButton.Height = actualHeightTextAndButtons / 2;
        HexAngleIncrimentButton.Width = actualWidthUpAndDownButtons;
        HexAngleDecrementButton.Height = actualHeightTextAndButtons / 2;
        HexAngleDecrementButton.Width = actualWidthUpAndDownButtons;

        TriangleUpHexAngle.Height = actualHeightTextAndButtons / 2 - 5;
        TriangleUpHexAngle.Width = actualWidthUpAndDownButtons / 2 - 5;
        TriangleDownHexAngle.Height = actualHeightTextAndButtons / 2 - 5;
        TriangleDownHexAngle.Width = actualWidthUpAndDownButtons / 2 - 5;
    }

    private void RectanglesGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint pointControlPosition = e.GetCurrentPoint(RectanglesGrid);

        PixelPoint gridPosition = GetGridPosition(pointControlPosition.Position);

        if (pointControlPosition.Properties.IsLeftButtonPressed)
        {
            RectanglesGrid_LeftButton(gridPosition);
        }
        else if (pointControlPosition.Properties.IsRightButtonPressed)
        {
            RectanglesGrid_RightButton(gridPosition);
        }
    }

    private PixelPoint GetGridPosition(Point mousePosition)
    {
        var tileSize = WindowMain.TileSet.TileSize;

        var position = new PixelPoint(
            (int)mousePosition.X / ((int)TileGrid.Width / tileSize.Width),
            (int)mousePosition.Y / ((int)TileGrid.Height / tileSize.Height));

        return position;
    }

    private void RectanglesGrid_LeftButton(PixelPoint gridPosition)
    {
        if (isTileEditorMode)
        {
            WindowMain.EditTile(gridPosition, true);
            return;
        }

        RectanglesGridUpdate(gridPosition, blueAndGreenSquare.Item1, blueAndGreenSquare.Item2);
    }

    private void RectanglesGrid_RightButton(PixelPoint gridPosition)
    {
        if (isTileEditorMode)
        {
            WindowMain.EditTile(gridPosition, false);
            return;
        }

        RectanglesGridUpdate(gridPosition, blueAndGreenSquare.Item2, blueAndGreenSquare.Item1);
    }

    private void RectanglesGridUpdate(PixelPoint gridPosition,
        SquareAndPosition firstSquare, SquareAndPosition secondSquare)
    {
        if (WindowMain.AngleMap.Values.Count <= 0)
        {
            return;
        }

        SquaresUtilities.MoveSquare(WindowMain.Window, gridPosition, firstSquare, secondSquare);

        if (RectanglesGrid.Children.Contains(firstSquare.Square)
            && RectanglesGrid.Children.Contains(secondSquare.Square))
        {
            WindowMain.UpdateAngles(blueAndGreenSquare.Item1.Position, blueAndGreenSquare.Item2.Position);
            DrawRedLine();
        }
    }

    private void SelectTileTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            WindowMain.SelectTile();
        }
    }

    private async void RectanglesGridUpdate(bool isAppear)
    {
        isPointerInRectanglesGrid = isAppear;
        while (isAppear && RectanglesGrid.Opacity < 1d || !isAppear && RectanglesGrid.Opacity > 0d)
        {
            if (isPointerInRectanglesGrid != isAppear)
            {
                return;
            }

            await Task.Delay(10);
            RectanglesGrid.Opacity = Math.Clamp(RectanglesGrid.Opacity + (isAppear ? 0.05 : -0.05), 0d, 1d);
        }
    }

    private void RectanglesGrid_MouseEnter(object sender, PointerEventArgs e)
    {
        RectanglesGridUpdate(true);
    }

    private void RectanglesGrid_MouseLeave(object sender, PointerEventArgs e)
    {
        RectanglesGridUpdate(false);
    }

    private int GetUniformGridIndex(Point mousePosition)
    {
        var tileSize = new PixelSize(
            WindowMain.TileSet.TileSize.Width * TileMapTileScale + tileMapSeparation,
            WindowMain.TileSet.TileSize.Height * TileMapTileScale + tileMapSeparation);

        return (int)mousePosition.X / tileSize.Width
            + (int)mousePosition.Y / tileSize.Height * TileMapGrid.Columns;
    }

    private void TileMapGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowMain.TileSet.Tiles.Count <= 0 ||
            !e.GetCurrentPoint(RectanglesGrid).Properties.IsLeftButtonPressed)
        {
            return;
        }

        ((Border)TileMapGrid.Children[WindowMain.SelectedTile]).BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(211, 211, 211));

        var mousePosition = e.GetPosition(TileMapGrid);

        WindowMain.SelectedTile = GetUniformGridIndex(mousePosition);

        if (WindowMain.SelectedTile > WindowMain.TileSet.Tiles.Count - 1)
        {
            WindowMain.SelectedTile = WindowMain.TileSet.Tiles.Count - 1;
        }

        ((Border)TileMapGrid.Children[WindowMain.SelectedTile]).BorderBrush = new SolidColorBrush(Colors.Red);

        WindowMain.SelectTileFromTileMap();
        LastSelectedTile = WindowMain.SelectedTile;
    }

    private void ModSwitchButtonClick(object? sender, RoutedEventArgs e)
    {
        if (isTileEditorMode)
        {
            isTileEditorMode = false;
            ModSwitchButton.Content = "Angle mode";
            DrawRedLine();
            return;
        }

        isTileEditorMode = true;
        ModSwitchButton.Content = "Editor mode";

        canvasForLine.Children.Clear();
        RectanglesGrid.Children.Clear();
    }
}
