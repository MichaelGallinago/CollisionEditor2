using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CollisionEditor2.Models;
using CollisionEditor2.ViewModels;
using CollisionEditor2.ViewServices;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

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
        private Line redLine = new Line();

        private MainViewModel windowMain { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            windowMain = new MainViewModel(this);
        }

        private Vector2<int> GetGridPosition(Point mousePosition, Grid grid)
        {
            Vector2<int> position = new Vector2<int>();

            var tileSize = windowMain.TileSet.TileSize;
            int tileWidth = tileSize.Width;
            int tileHeight = tileSize.Height;
            position.X = (int)mousePosition.X / tileWidth;
            position.Y = (int)mousePosition.Y / tileHeight;
            return position;


            //foreach (var column in grid.ColumnDefinitions)
            //{
            //    if (mousePosition.X > column.Offset && mousePosition.X < (column.Offset + column.ActualWidth))
            //        break;
            //    position.X++;
            //}

            //foreach (var row in grid.RowDefinitions)
            //{
            //    if (mousePosition.Y > row.Offset && mousePosition.Y < (row.Offset + row.ActualHeight))
            //        break;
            //    position.Y++;
            //}

        }

        private void RectanglesGrid_MouseLeftButtonDown(PointerPressedEventArgs e)
        {
            RectanglesGridUpdate(e, blueAndGreenSquare.Item1, blueAndGreenSquare.Item2);
        }

        private void RectanglesGrid_MouseRightButtonDown(PointerPressedEventArgs e)
        {
            RectanglesGridUpdate(e, blueAndGreenSquare.Item2, blueAndGreenSquare.Item1);
        }

        private void RectanglesGridUpdate(PointerEventArgs e, SquareAndPosition firstSquare, SquareAndPosition secondSquare)
        {
            if (windowMain.AngleMap.Values.Count <= 0)
            {
                return;
            }

            var mousePosition = e.GetPosition(RectanglesGrid);
            Vector2<int> position = GetGridPosition(mousePosition, RectanglesGrid);

            SquaresService.MoveSquare(windowMain.window, position, firstSquare, secondSquare);

            if (RectanglesGrid.Children.Contains(firstSquare.Square) && RectanglesGrid.Children.Contains(secondSquare.Square))
            {
                windowMain.UpdateAngles(blueAndGreenSquare.Item1.Position, blueAndGreenSquare.Item2.Position);
                DrawRedLine();
            }
        }

        internal void DrawRedLine()
        {
            if (windowMain.AngleMap.Values.Count > 0)
                RedLineService.DrawRedLine(windowMain.window, ref redLine);
        }

        private void SelectTileTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                windowMain.SelectTile();
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

        private void TextBoxHexAngle_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrlKeyDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            Key[] exceptions = new Key[] { Key.Back, Key.Delete, Key.Left, Key.Right };

            if (TextBoxHexAngle.Text.Length >= 4 && !exceptions.Contains(e.Key) && !isCtrlKeyDown
                || TextBoxHexAngle.Text.Length > 0 && e.Key == Key.C && isCtrlKeyDown)
            {
                e.Handled = true;
            }
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
            var tileSize = windowMain.TileSet.TileSize;
            uint tileWidth = (uint)tileSize.Width * tileMapTileScale;
            uint tileHeight = (uint)tileSize.Height * tileMapTileScale;
            return (uint)mousePosition.X / (tileWidth  + tileMapSeparation) 
                + ((uint)mousePosition.Y / (tileHeight + tileMapSeparation)) * (uint)TileMapGrid.Columns;
        }

        private void TileMapGrid_MouseLeftButtonDown(PointerPressedEventArgs e)
        {
            if (windowMain.TileSet.Tiles.Count <= 0)
                return;

            Image lastTile = windowMain.GetTile((int)windowMain.ChosenTile);

            TileMapGrid.Children.RemoveAt((int)windowMain.ChosenTile);
            TileMapGrid.Children.Insert((int)windowMain.ChosenTile, lastTile);

            var mousePosition = e.GetPosition(TileMapGrid);

            windowMain.ChosenTile = GetUniformGridIndex(mousePosition);

            if (windowMain.ChosenTile > windowMain.TileSet.Tiles.Count - 1)
                windowMain.ChosenTile = (uint)windowMain.TileSet.Tiles.Count - 1;

            Image newTile = windowMain.GetTile((int)windowMain.ChosenTile);

            var tileSize = windowMain.TileSet.TileSize;
            Border border = new Border()
            {
                Width = tileSize.Width * tileMapTileScale + tileMapSeparation,
                Height = tileSize.Height * tileMapTileScale + tileMapSeparation,
                BorderBrush = new SolidColorBrush(Colors.Red),
                BorderThickness = new Thickness(2),
                Child = newTile
            };

            TileMapGrid.Children.RemoveAt((int)windowMain.ChosenTile);
            TileMapGrid.Children.Insert((int)windowMain.ChosenTile, border);

            windowMain.SelectTileFromTileMap();
            LastChosenTile = (int)windowMain.ChosenTile;
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (Equals(e.Source, TileMapGrid))
                {
                    TileMapGrid_MouseLeftButtonDown(e);
                }
                else if (Equals(e.Source, TileMapGrid))
                {
                    RectanglesGrid_MouseLeftButtonDown(e);
                }
            }

            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                if (Equals(e.Source, TileMapGrid))
                {
                    RectanglesGrid_MouseRightButtonDown(e);
                }
            }
        }

        private void WindowSizeChanged(object sender, EventArgs e)
        {
            int countOfTiles = windowMain.TileSet.Tiles.Count;
            var tileSize = windowMain.TileSet.TileSize;

            double actualHeightTextAndButtons = (Height - menuHeight) / countHeightParts * textAndButtonsHeight;
            double actualWidthUpAndDownButtons = Width / countWidthParts * upAndDownButtonsWidth;
            double actualFontSize = Math.Min((25.4 / 96 * actualHeightTextAndButtons) / 0.35 - 4, (25.4 / 96 * (Width / countHeightParts * 43)) / 0.35 - 21);

            double actualHeightGrid = (Height - menuHeight) / countHeightParts * gridHeight;

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

            TileMapGrid.Width   = baseTileMapGridWidth + (((int)(Width / countWidthParts * tileMapGridWidth) - startTileMapGridWidth) / tileWidth) * tileWidth;
            TileMapGrid.Columns = ((int)TileMapGrid.Width + tileMapSeparation) / (tileWidth + tileMapSeparation);
            TileMapGrid.Height  = (int)Math.Ceiling((double)countOfTiles / TileMapGrid.Columns) * (tileHeight + tileMapSeparation);

            SelectTileTextBox.Height   = actualHeightTextAndButtons - 2;
            SelectTileTextBox.FontSize = actualFontSize;
            SelectTileButton.Height    = actualHeightTextAndButtons - 2;
            SelectTileButton.FontSize  = actualFontSize;

            DrawRedLine();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://youtu.be/m5sbRiwQPMQ?t=87",
                UseShellExecute = true,
            });
        }
    }
}