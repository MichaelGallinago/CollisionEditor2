using CollisionEditor2.ViewServices;
using CollisionEditor2.ViewModels;
using CollisionEditor2.Models;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Reactive.Linq;

namespace CollisionEditor2.Views
{
    public partial class MainWindow : Window
    {
        public int LastChosenTile { get; set; }

        private const int tileMapSeparation = 4;
        private const int tileMapTileScale = 2;
        private const int menuHeight = 20;
        private const int textAndButtonsHeight = 20;
        private const int upAndDownButtonsWidth = 23;
        private const int gridHeight = 128;
        private const int tileMapGridWidth = 278;
        private const int countHeightParts = 404;
        private const int countWidthParts = 587;
        private const int baseTileMapGridWidth = 288;
        private const int startTileMapGridWidth = 314;

        private bool mouseInRectanglesGrid = false;
        private (SquareAndPosition, SquareAndPosition) blueAndGreenSquare = (new SquareAndPosition(Colors.Blue), new SquareAndPosition(Colors.Green));
        private Line redLine = new();

        public MainViewModel WindowMain { get; }

        public MainWindow()
        {
            InitializeComponent();
            WindowMain = new MainViewModel(this);
            this.GetObservable(ClientSizeProperty).Subscribe(WindowSizeChanged);
        }

        private Vector2<int> GetGridPosition(Vector2<double> mousePosition)
        {
            Vector2<int> position = new();

            var tileSize = WindowMain.TileSet.TileSize;
            position.X = (int)Math.Floor(2 * mousePosition.X / tileSize.Width);
            position.Y = (int)Math.Floor(2 * mousePosition.Y / tileSize.Height);
            return position;
        }

        public void RectanglesGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pointControlPosition = e.GetCurrentPoint(RectanglesGrid);
            var x = pointControlPosition.Position.X;
            var y = pointControlPosition.Position.Y;

            if (pointControlPosition.Properties.IsLeftButtonPressed)
            {
                RectanglesGrid_LeftButton(x, y);
            }
            else if (pointControlPosition.Properties.IsRightButtonPressed)
            {
                RectanglesGrid_RightButton(x, y);
            }
        }

        private void RectanglesGrid_LeftButton(double x, double y)
        {
            var mousePosition = new Vector2<double>(x, y);
            RectanglesGridUpdate(mousePosition, blueAndGreenSquare.Item1, blueAndGreenSquare.Item2);
        }

        private void RectanglesGrid_RightButton(double x, double y)
        {
            var mousePosition = new Vector2<double>(x, y);
            RectanglesGridUpdate(mousePosition, blueAndGreenSquare.Item2, blueAndGreenSquare.Item1);
        }

        private void RectanglesGridUpdate(Vector2<double> mousePosition, SquareAndPosition firstSquare, SquareAndPosition secondSquare)
        {
            if (WindowMain.AngleMap.Values.Count <= 0)
            {
                return;
            }
            
            Vector2<int> position = GetGridPosition(mousePosition);

            SquaresService.MoveSquare(WindowMain.window, position, firstSquare, secondSquare);

            if (RectanglesGrid.Children.Contains(firstSquare.Square) && RectanglesGrid.Children.Contains(secondSquare.Square))
            {
                WindowMain.UpdateAngles(blueAndGreenSquare.Item1.Position, blueAndGreenSquare.Item2.Position);
                DrawRedLine();
            }
        }

        internal void DrawRedLine()
        {
            if (WindowMain.AngleMap.Values.Count > 0)
                RedLineService.DrawRedLine(WindowMain.window, ref redLine);
        }

        private void SelectTileTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                WindowMain.SelectTile();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Keyboard.Keys.Add(e.Key);
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Keyboard.Keys.Remove(e.Key);
            base.OnKeyUp(e);
        }

        private async void RectanglesGridUpdate(bool isAppear)
        {
            mouseInRectanglesGrid = isAppear;
            while (isAppear && RectanglesGrid.Opacity < 1d || !isAppear && RectanglesGrid.Opacity > 0d)
            {
                if (mouseInRectanglesGrid != isAppear)
                    return;

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

        private uint GetUniformGridIndex(Point mousePosition)
        {
            var tileSize = WindowMain.TileSet.TileSize;
            uint tileWidth = (uint)tileSize.Width * tileMapTileScale;
            uint tileHeight = (uint)tileSize.Height * tileMapTileScale;
            return (uint)mousePosition.X / (tileWidth  + tileMapSeparation) 
                + ((uint)mousePosition.Y / (tileHeight + tileMapSeparation)) * (uint)TileMapGrid.Columns;
        }

        public void TileMapGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (WindowMain.TileSet.Tiles.Count <= 0 || 
                !e.GetCurrentPoint(RectanglesGrid).Properties.IsLeftButtonPressed)
            {
                return;
            }

            Image lastTile = WindowMain.GetTile((int)WindowMain.ChosenTile);

            TileMapGrid.Children.RemoveAt((int)WindowMain.ChosenTile);
            TileMapGrid.Children.Insert((int)WindowMain.ChosenTile, lastTile);

            var mousePosition = e.GetPosition(TileMapGrid);

            WindowMain.ChosenTile = GetUniformGridIndex(mousePosition);

            if (WindowMain.ChosenTile > WindowMain.TileSet.Tiles.Count - 1)
                WindowMain.ChosenTile = (uint)WindowMain.TileSet.Tiles.Count - 1;

            Image newTile = WindowMain.GetTile((int)WindowMain.ChosenTile);

            var tileSize = WindowMain.TileSet.TileSize;
            var border = new Border()
            {
                Width = tileSize.Width * tileMapTileScale + tileMapSeparation,
                Height = tileSize.Height * tileMapTileScale + tileMapSeparation,
                BorderBrush = new SolidColorBrush(Colors.Red),
                BorderThickness = new Thickness(2),
                Child = newTile
            };

            TileMapGrid.Children.RemoveAt((int)WindowMain.ChosenTile);
            TileMapGrid.Children.Insert((int)WindowMain.ChosenTile, border);

            WindowMain.SelectTileFromTileMap();
            LastChosenTile = (int)WindowMain.ChosenTile;
        }

        private void WindowSizeChanged(Size size)
        {
            int countOfTiles = WindowMain.TileSet.Tiles.Count;
            var tileSize = WindowMain.TileSet.TileSize;

            double actualHeightTextAndButtons = (size.Height - menuHeight) / countHeightParts * textAndButtonsHeight;
            double actualWidthUpAndDownButtons = size.Width / countWidthParts * upAndDownButtonsWidth;
            double actualFontSize = Math.Min((25.4 / 96 * actualHeightTextAndButtons) / 0.35 - 4, (25.4 / 96 * (size.Width / countHeightParts * 43)) / 0.35 - 21);

            double actualHeightGrid = (size.Height - menuHeight) / countHeightParts * gridHeight;

            TileGrid.Width  = actualHeightGrid;
            TileGrid.Height = actualHeightGrid;

            RectanglesGrid.Width  = actualHeightGrid;
            RectanglesGrid.Height = actualHeightGrid;

            canvasForLine.Width  = actualHeightGrid;
            canvasForLine.Height = actualHeightGrid;

            Heights.Height   = actualHeightTextAndButtons;
            Heights.FontSize = actualFontSize;

            Widths.Height   = actualHeightTextAndButtons;
            Widths.FontSize = actualFontSize;

            TextBlockFullAngle.Height   = actualHeightTextAndButtons - 2;
            TextBlockFullAngle.FontSize = actualFontSize;

            TextBoxByteAngle.Height   = actualHeightTextAndButtons - 2;
            TextBoxByteAngle.FontSize = actualFontSize;

            TextBoxHexAngle.Height   = actualHeightTextAndButtons - 2;
            TextBoxHexAngle.FontSize = actualFontSize;


            ByteAngleIncrimentButton.Height = actualHeightTextAndButtons / 2;
            ByteAngleIncrimentButton.Width  = actualWidthUpAndDownButtons - 3;
            ByteAngleDecrementButton.Height = actualHeightTextAndButtons / 2 - 1;
            ByteAngleDecrementButton.Width  = actualWidthUpAndDownButtons - 3;

            TriangleUpByteAngle.Height   = actualHeightTextAndButtons  / 2 - 5;
            TriangleUpByteAngle.Width    = actualWidthUpAndDownButtons / 2 - 5;
            TriangleDownByteAngle.Height = actualHeightTextAndButtons  / 2 - 5;
            TriangleDownByteAngle.Width  = actualWidthUpAndDownButtons / 2 - 5;

            HexAngleIncrimentButton.Height = actualHeightTextAndButtons / 2;
            HexAngleIncrimentButton.Width  = actualWidthUpAndDownButtons - 3;
            HexAngleDecrementButton.Height = actualHeightTextAndButtons / 2 - 1;
            HexAngleDecrementButton.Width  = actualWidthUpAndDownButtons - 3;

            TriangleUpHexAngle.Height   = actualHeightTextAndButtons  / 2 - 5;
            TriangleUpHexAngle.Width    = actualWidthUpAndDownButtons / 2 - 5;
            TriangleDownHexAngle.Height = actualHeightTextAndButtons  / 2 - 5;
            TriangleDownHexAngle.Width  = actualWidthUpAndDownButtons / 2 - 5;

            int tileWidth  = tileSize.Width  * tileMapTileScale;
            int tileHeight = tileSize.Height * tileMapTileScale;

            TileMapGrid.Width   = baseTileMapGridWidth + (((int)(size.Width / countWidthParts * tileMapGridWidth) - startTileMapGridWidth) / tileWidth) * tileWidth;
            TileMapGrid.Columns = ((int)TileMapGrid.Width + tileMapSeparation) / (tileWidth + tileMapSeparation);
            TileMapGrid.Height  = (int)Math.Ceiling((double)countOfTiles / TileMapGrid.Columns) * (tileHeight + tileMapSeparation);

            SelectTileTextBox.Height   = actualHeightTextAndButtons - 2;
            SelectTileTextBox.FontSize = actualFontSize;
            SelectTileButton.Height    = actualHeightTextAndButtons - 2;
            SelectTileButton.FontSize  = actualFontSize;

            DrawRedLine();
        }
    }
}