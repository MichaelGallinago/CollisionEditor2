using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CollisionEditor2.Models;
using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.Views;
using CollisionEditor2.ViewUtilities;
using MessageBoxSlim.Avalonia;
using MessageBoxSlim.Avalonia.DTO;
using MessageBoxSlim.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace CollisionEditor2.ViewModels;

public class MainViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private const int tileMapSeparation = 4;
    private const double tileGridBorderThickness = 1d;
    private const double thicknessToSeparationRatio = 2d;

    private byte byteAngle;
    private string hexAngle;
    private readonly TextboxValidator textboxValidator;

    public AngleMap AngleMap { get; private set; }
    public TileSet TileSet { get; private set; }

    public ReactiveCommand<Unit, Unit>? MenuOpenAngleMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuOpenTileMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuSaveTileMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuSaveWidthMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuSaveHeightMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuSaveAngleMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuSaveAllCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuUnloadTileMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuUnloadAngleMapCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? MenuUnloadAllCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? SelectTileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? AngleIncrementCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? AngleDecrementCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? AddTileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? DeleteTileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? ExitAppCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? HelpCommand { get; private set; }

    public int SelectedTile { get; set; }

    public MainWindow Window { get; set; }

    public MainViewModel(MainWindow window)
    {
        textboxValidator = new TextboxValidator();

        textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;

        AngleMap = new AngleMap();
        TileSet = new TileSet();

        byteAngle = 0;
        hexAngle = "0x00";
        Window = window;

        SetMenuCommand();
        SetAngleIncAndDecCommand();
        SetRightPanelCommand();
        ExitAppCommand = ReactiveCommand.Create(ExitApp);

        RectanglesGridUpdate();

        TileGridUpdate(TileSet, SelectedTile, window);
    }
    public string ByteAngleText
    {
        get => byteAngle.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(ByteAngleText));
            if (!byte.TryParse(value, out byte newAngle))
            {
                textboxValidator.AddError(nameof(ByteAngleText), "Wrong byte number");
                return;
            }

            byteAngle = newAngle;

            ShowAngles(Angles.FromByte(byteAngle));
            AngleMap.SetAngle(SelectedTile, byteAngle);

            Window.DrawRedLine();
        }
    }

    public string HexAngleText
    {
        get => hexAngle;
        set
        {
            if (!ValidateHexAngle(value))
            {
                return;
            }

            hexAngle = value;

            Angles angles = Angles.FromHex(hexAngle);
            AngleMap.SetAngle(SelectedTile, angles.ByteAngle);
            ShowAngles(angles);

            Window.DrawRedLine();
        }
    }

    public string SelectedTileText
    {
        get => SelectedTile.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(SelectedTileText));

            if (!int.TryParse(value, out int newSelectedTile) || newSelectedTile < 0)
            {
                textboxValidator.AddError(nameof(SelectedTileText), "Wrong tile index");
                return;
            }

            SelectedTile = newSelectedTile;
        }
    }

    public void SelectTile()
    {
        if (SelectedTile > TileSet.Tiles.Count - 1)
        {
            SelectedTile = TileSet.Tiles.Count - 1;
            OnPropertyChanged(nameof(SelectedTileText));
        }

        ((Border)Window.TileMapGrid.Children[Window.LastSelectedTile]).BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(211, 211, 211));

        ((Border)Window.TileMapGrid.Children[SelectedTile]).BorderBrush = new SolidColorBrush(Colors.Red);

        Window.LastSelectedTile = SelectedTile;

        TileGridUpdate(TileSet, SelectedTile, Window);

        SetHeightsAndWidths();

        ShowAngles(Angles.FromByte(AngleMap.Values[SelectedTile]));

        Window.DrawRedLine();
        Window.RectanglesGrid.Children.Clear();
    }
    public void SelectTileFromTileMap()
    {
        OnPropertyChanged(nameof(SelectedTileText));
        TileGridUpdate(TileSet, SelectedTile, Window);

        SetHeightsAndWidths();

        ShowAngles(Angles.FromByte(AngleMap.Values[SelectedTile]));

        Window.DrawRedLine();
        Window.RectanglesGrid.Children.Clear();
    }
    public void UpdateAngles(PixelPoint positionGreen, PixelPoint positionBlue)
    {
        if (AngleMap.Values.Count <= 0)
        {
            return;
        }

        byte byteAngle = AngleMap.SetAngleFromLine(SelectedTile, positionGreen, positionBlue);

        ShowAngles(Angles.FromByte(byteAngle));
    }
    public void EditTile(PixelPoint tilePosition, bool isLeftButtonPressed)
    {
        TileSet.ChangeTile(SelectedTile, tilePosition, isLeftButtonPressed);
        TileGridUpdate(TileSet, SelectedTile, Window);

        SetHeightsAndWidths();

        Border newTile = GetTile(SelectedTile);
        newTile.BorderBrush = new SolidColorBrush(Colors.Red);
        Window.TileMapGrid.Children[SelectedTile] = newTile;
    }
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public IEnumerable GetErrors(string? propertyName)
    {
        return textboxValidator.GetErrors(propertyName);
    }
    public bool HasErrors => textboxValidator.HasErrors;

    private void SetMenuCommand()
    {
        MenuOpenAngleMapCommand = ReactiveCommand.Create(MenuOpenAngleMap);
        MenuOpenTileMapCommand = ReactiveCommand.Create(MenuOpenTileMap);

        MenuSaveTileMapCommand = ReactiveCommand.Create(MenuSaveTileMap);
        MenuSaveWidthMapCommand = ReactiveCommand.Create(MenuSaveWidthMap);
        MenuSaveHeightMapCommand = ReactiveCommand.Create(MenuSaveHeightMap);
        MenuSaveAngleMapCommand = ReactiveCommand.Create(MenuSaveAngleMap);
        MenuSaveAllCommand = ReactiveCommand.Create(MenuSaveAll);

        MenuUnloadTileMapCommand = ReactiveCommand.Create(MenuUnloadTileMap);
        MenuUnloadAngleMapCommand = ReactiveCommand.Create(MenuUnloadAngleMap);
        MenuUnloadAllCommand = ReactiveCommand.Create(MenuUnloadAll);

        HelpCommand = ReactiveCommand.Create(Help);
    }
    private void SetAngleIncAndDecCommand()
    {
        AngleIncrementCommand = ReactiveCommand.Create(AngleIncrement);
        AngleDecrementCommand = ReactiveCommand.Create(AngleDecrement);
    }
    private void SetRightPanelCommand()
    {
        SelectTileCommand = ReactiveCommand.Create(SelectTile);

        AddTileCommand = ReactiveCommand.Create(AddTile);
        DeleteTileCommand = ReactiveCommand.Create(DeleteTile);
    }

    private async void MenuOpenAngleMap()
    {
        string filePath = await ViewModelFileUtilities.GetFileOpenPath(Window, ViewModelFileUtilities.Filters.AngleMap);
        if (filePath == string.Empty)
        {
            return;
        }

        AngleMap = new AngleMap(filePath);
        if (TileSet.Tiles.Count <= 0)
        {
            TileSet = new TileSet(AngleMap.Values.Count);
        }

        ViewModelAssistant.SupplementElements(AngleMap, TileSet);

        ShowAngles(Angles.FromByte(AngleMap.Values[SelectedTile]));
        EnableRightPanelAndModSwitchButtons();

        TileMapGridReset();
        TileMapGridHeightUpdate(TileSet.Tiles.Count);
        Window.DrawRedLine();
        SelectTile();
    }

    private void ShowAngles(Angles angles)
    {
        Window.TextBoxByteAngle.IsEnabled = true;
        Window.TextBoxHexAngle.IsEnabled = true;

        Window.BorderFullAngle.BorderBrush = new SolidColorBrush(Color.FromRgb(84, 84, 84));
        Window.BorderFullAngle.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
        Window.TextBlockFullAngle.Foreground = new SolidColorBrush(Colors.Black);
        Window.TextBlockFullAngle.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));

        byteAngle = angles.ByteAngle;
        OnPropertyChanged(nameof(ByteAngleText));
        textboxValidator.ClearErrors(nameof(ByteAngleText));
        Window.ByteAngleIncrimentButton.IsEnabled = true;
        Window.ByteAngleDecrementButton.IsEnabled = true;

        hexAngle = angles.HexAngle;
        OnPropertyChanged(nameof(HexAngleText));
        textboxValidator.ClearErrors(nameof(HexAngleText));
        Window.HexAngleIncrimentButton.IsEnabled = true;
        Window.HexAngleDecrementButton.IsEnabled = true;

        Window.TextBlockFullAngle.Text = angles.FullAngle + "°";
    }

    private bool ValidateHexAngle(string hexAngle)
    {
        textboxValidator.ClearErrors(nameof(HexAngleText));

        if (hexAngle.Length <= Angles.HexAnglePrefixLength || hexAngle.Length > Angles.HexAnglePrefixLength + Angles.HexAngleMaxLength)
        {
            textboxValidator.AddError(nameof(HexAngleText),
                $"Wrong hexadecimal number length!\nMust be between {Angles.HexAnglePrefixLength + 1} and "
                + $"{Angles.HexAnglePrefixLength + Angles.HexAngleMaxLength}");
            return false;
        }

        string prefix = hexAngle[..Angles.HexAnglePrefixLength];
        if (prefix != "0x" && prefix != "0X")
        {
            textboxValidator.AddError(nameof(HexAngleText),
                "Wrong hexadecimal prefix!\nMust be '0x' or '0X'");
            return false;
        }

        if (!int.TryParse(hexAngle[Angles.HexAnglePrefixLength..],
            System.Globalization.NumberStyles.HexNumber, null, out _))
        {
            textboxValidator.AddError(nameof(HexAngleText),
                "Wrong hexadecimal number alphabet!\nMust be '0123456789ABCDEFabcdef'");
            return false;
        }

        return true;
    }

    private async void MenuOpenTileMap()
    {
        TileSet? tileSet = await CreateTileSetFromOpenTileMapWindow();
        if (tileSet is null)
        {
            return;
        }
        TileSet = tileSet;

        if (AngleMap.Values.Count <= 0)
        {
            AngleMap = new AngleMap(TileSet.Tiles.Count);
        }

        ViewModelAssistant.SupplementElements(AngleMap, TileSet);

        TileGridUpdate(TileSet, SelectedTile, Window);
        RectanglesGridUpdate();
        SetHeightsAndWidths();
        ShowAngles(Angles.FromByte(AngleMap.Values[SelectedTile]));
        EnableRightPanelAndModSwitchButtons();

        TileMapGridReset();
        TileMapGridHeightUpdate(TileSet.Tiles.Count);

        SelectTile();

        Window.DrawRedLine();
        Window.WindowSizeChanged(new Size(Window.Width, Window.Height));
    }

    private async Task<TileSet?> CreateTileSetFromOpenTileMapWindow()
    {
        string filePath = await ViewModelFileUtilities.GetFileOpenPath(Window, ViewModelFileUtilities.Filters.TileMap);
        if (filePath == string.Empty)
        {
            return null;
        }

        OpenTileMap openTileMap = new();
        openTileMap.DataContext = new OpenTileMapViewModel(openTileMap, filePath);
        await openTileMap.ShowDialog(Window);
        if (!openTileMap.IsOpened)
        {
            return null;
        }

        return new TileSet(filePath, openTileMap.TileWidth, openTileMap.TileHeight,
            new PixelSize(openTileMap.HorizontalSeparation, openTileMap.VerticalSeparation),
            new PixelSize(openTileMap.HorizontalOffset, openTileMap.VerticalOffset));
    }

    private void SetHeightsAndWidths()
    {
        Window.Heights.Text = TileUtilities.GetCollisionValues(TileSet.Tiles[SelectedTile].Heights);
        Window.Widths.Text = TileUtilities.GetCollisionValues(TileSet.Tiles[SelectedTile].Widths);
    }

    private void EnableRightPanelAndModSwitchButtons()
    {
        Window.ModSwitchButton.IsEnabled = true;
        Window.SelectTileTextBox.IsEnabled = true;
        Window.SelectTileButton.IsEnabled = true;
        Window.AddTileButton.IsEnabled = true;
        Window.DeleteTileButton.IsEnabled = true;
    }

    private void TileMapGridReset()
    {
        Window.TileMapGrid.Children.Clear();

        for (int i = 0; i < TileSet.Tiles.Count; i++)
        {
            Window.TileMapGrid.Children.Add(GetTile(i));
        }
    }

    private void TileMapGridHeightUpdate(int tileCount)
    {
        Window.TileMapGrid.Height = (int)Math.Ceiling((double)tileCount / Window.TileMapGrid.Columns)
            * (TileSet.TileSize.Height * MainWindow.TileMapTileScale + tileMapSeparation);
    }

    private async void OurMessageBox(string message)
    {
        _ = await BoxedMessage.Create(new MessageBoxParams
        {
            Buttons = ButtonEnum.Ok,
            ContentTitle = "Error",
            ContentMessage = message,
            Icon = new Avalonia.Media.Imaging.Bitmap("../../../../CollisionEditor2/Assets/avalonia-logo.ico"),
            Location = WindowStartupLocation.CenterScreen,
            CanResize = false,
        }).ShowDialogAsync(Window);
    }

    private async void MenuSaveTileMap()
    {
        if (TileSet.Tiles.Count <= 0)
        {
            OurMessageBox("Error: You haven't selected TileMap to save");
            return;
        }

        SaveTileMap saveTileMap = new();
        saveTileMap.DataContext = new SaveTileMapViewModel(saveTileMap, TileSet);
        await saveTileMap.ShowDialog(Window);
        if (!saveTileMap.IsSaved)
        {
            return;
        }

        if (saveTileMap.ResultSaveImage == null)
        {
            OurMessageBox("Your Image is NULL!");
            return;
        }

        string filePath = await ViewModelFileUtilities.GetFileSavePath(Window, ViewModelFileUtilities.Filters.TileMap);
        if (filePath != string.Empty)
        {
            TileUtilities.SaveTileMap(Path.GetFullPath(filePath), saveTileMap.ResultSaveImage);
        }
    }

    private async void MenuSaveWidthMap()
    {
        if (TileSet.Tiles.Count <= 0)
        {
            OurMessageBox("Error: The WidthMap isn't generated!");
            return;
        }

        string filePath = await ViewModelFileUtilities.GetFileSavePath(Window, ViewModelFileUtilities.Filters.WidthMap);
        if (filePath != string.Empty)
        {
            TileUtilities.SaveCollisionMap(Path.GetFullPath(filePath), TileSet.Tiles, true);
        }
    }

    private async void MenuSaveHeightMap()
    {
        if (TileSet.Tiles.Count <= 0)
        {
            OurMessageBox("Error: The HeightMap isn't generated!");
            return;
        }

        string filePath = await ViewModelFileUtilities.GetFileSavePath(Window, ViewModelFileUtilities.Filters.HeightMap);
        if (filePath != string.Empty)
        {
            TileUtilities.SaveCollisionMap(Path.GetFullPath(filePath), TileSet.Tiles, false);
        }
    }

    private async void MenuSaveAngleMap()
    {
        if (AngleMap.Values.Count <= 0)
        {
            OurMessageBox("Error: You haven't selected AngleMap to save");
            return;
        }

        string filePath = await ViewModelFileUtilities.GetFileSavePath(Window, ViewModelFileUtilities.Filters.AngleMap);
        if (filePath != string.Empty)
        {
            AngleMap.Save(Path.GetFullPath(filePath));
        }
    }

    private void MenuSaveAll()
    {
        MenuSaveAngleMap();
        MenuSaveHeightMap();
        MenuSaveWidthMap();
        MenuSaveTileMap();
    }

    private void MenuUnloadTileMap()
    {
        var tile = TileSet.Tiles[SelectedTile];
        TileSet = new TileSet(AngleMap.Values.Count, tile.Heights.Length, tile.Widths.Length);

        TileMapGridReset();

        TileGridUpdate(TileSet, SelectedTile, Window);

        SetHeightsAndWidths();

        ShowAngles(Angles.FromByte(AngleMap.Values[SelectedTile]));
    }

    private void MenuUnloadAngleMap()
    {
        AngleMap = new AngleMap(TileSet.Tiles.Count);
        ShowAngles(Angles.FromByte(AngleMap.Values[SelectedTile]));
        Window.RectanglesGrid.Children.Clear();
        Window.DrawRedLine();
    }

    private void MenuUnloadAll()
    {
        Window.TileMapGrid.Children.Clear();

        TileSet = new TileSet();
        AngleMap = new AngleMap();

        Window.Heights.Text = null;
        Window.Widths.Text = null;

        ShowAngles(Angles.FromByte(0));

        SelectedTile = 0;
        OnPropertyChanged(nameof(SelectedTileText));
        Window.LastSelectedTile = 0;
        AllButtonsAndTextBlocksUnenabled();

        Window.canvasForLine.Children.Clear();
        Window.RectanglesGrid.Children.Clear();

        TileMapGridHeightUpdate(TileSet.Tiles.Count);
        TileGridUpdate(TileSet, 0, Window);
    }

    private void AllButtonsAndTextBlocksUnenabled()
    {
        Window.BorderFullAngle.BorderBrush = new SolidColorBrush(Color.FromRgb(176, 176, 176));
        Window.BorderFullAngle.Background = new SolidColorBrush(Color.FromRgb(196, 196, 196));
        Window.TextBlockFullAngle.Foreground = new SolidColorBrush(Colors.Gray);
        Window.TextBlockFullAngle.Background = new SolidColorBrush(Color.FromRgb(196, 196, 196));

        Window.ByteAngleIncrimentButton.IsEnabled = false;
        Window.ByteAngleDecrementButton.IsEnabled = false;
        Window.HexAngleIncrimentButton.IsEnabled = false;
        Window.HexAngleDecrementButton.IsEnabled = false;

        Window.SelectTileTextBox.IsEnabled = false;
        Window.SelectTileButton.IsEnabled = false;
        Window.ModSwitchButton.IsEnabled = false;
        Window.AddTileButton.IsEnabled = false;
        Window.DeleteTileButton.IsEnabled = false;

        Window.TextBoxByteAngle.IsEnabled = false;
        Window.TextBoxHexAngle.IsEnabled = false;
    }

    private void AngleIncrement()
    {
        byte byteAngle = AngleMap.ChangeAngle(SelectedTile, 1);

        ShowAngles(Angles.FromByte(byteAngle));

        Window.DrawRedLine();
    }

    private void AngleDecrement()
    {
        byte byteAngle = AngleMap.ChangeAngle(SelectedTile, -1);

        ShowAngles(Angles.FromByte(byteAngle));

        Window.DrawRedLine();
    }

    private Border GetTile(int index)
    {
        Tile tile = TileSet.Tiles[index];

        var image = new Image
        {
            Width = TileSet.TileSize.Width * MainWindow.TileMapTileScale,
            Height = TileSet.TileSize.Height * MainWindow.TileMapTileScale,
            Source = ViewModelAssistant.GetBitmapFromPixelArray(
                ViewModelAssistant.GetPixelArrayFromTile(tile, new OurColor(0, 0, 0, 255)),
                new PixelSize(TileSet.Tiles[SelectedTile].Heights.Length, TileSet.Tiles[SelectedTile].Widths.Length))
        };

        var border = new Border()
        {
            Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(211, 211, 211)),
            BorderThickness = new Thickness(tileMapSeparation / thicknessToSeparationRatio),
            Child = image
        };

        return border;
    }

    private void AddTile()
    {
        if (SelectedTile > TileSet.Tiles.Count - 1)
        {
            SelectedTile = TileSet.Tiles.Count - 1;
            OnPropertyChanged(nameof(SelectedTileText));
        }

        if (SelectedTile < Window.LastSelectedTile)
        {
            Window.LastSelectedTile++;
        }

        SelectedTile++;
        OnPropertyChanged(nameof(SelectedTileText));

        TileSet.InsertTile(SelectedTile);
        AngleMap.InsertAngle(SelectedTile);

        Border newTile = GetTile(SelectedTile);
        Window.TileMapGrid.Children.Insert(SelectedTile, newTile);

        TileMapGridHeightUpdate(TileSet.Tiles.Count);
        SelectTile();
    }

    private void DeleteTile()
    {
        LimitSelectedTile();

        TileSet.RemoveTile(SelectedTile);
        AngleMap.RemoveAngle(SelectedTile);

        Window.TileMapGrid.Children.RemoveAt(SelectedTile);

        if (TileSet.Tiles.Count == 0)
        {
            MenuUnloadAll();
            return;
        }

        LimitSelectedTile();

        if (SelectedTile < Window.LastSelectedTile)
        {
            Window.LastSelectedTile--;
        }

        TileMapGridHeightUpdate(TileSet.Tiles.Count);
        SelectTile();
    }

    private void LimitSelectedTile()
    {
        if (SelectedTile >= TileSet.Tiles.Count)
        {
            SelectedTile = TileSet.Tiles.Count - 1;
            OnPropertyChanged(nameof(SelectedTileText));
        }
    }

    private void ExitApp()
    {
        Window.Close();
    }

    private void RectanglesGridUpdate()
    {
        Window.RectanglesGrid.ColumnDefinitions.Clear();
        Window.RectanglesGrid.RowDefinitions.Clear();

        var size = TileSet.TileSize;

        for (int x = 0; x < size.Width; x++)
        {
            Window.RectanglesGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (int y = 0; y < size.Height; y++)
        {
            Window.RectanglesGrid.RowDefinitions.Add(new RowDefinition());
        }
    }

    private static void TileGridUpdate(TileSet tileSet, int ChosenTile, MainWindow window)
    {
        window.TileGrid.Children.Clear();

        var size = tileSet.TileSize;

        window.TileGrid.Rows = size.Height;
        window.TileGrid.Columns = size.Width;
        window.TileGrid.Background = new SolidColorBrush(Colors.Transparent);

        Tile tile = tileSet.Tiles.Count > 0 ? tileSet.Tiles[ChosenTile] : new Tile(size);

        FillTileGrid(tile, size, window);
    }

    private static void FillTileGrid(Tile tile, PixelSize size, MainWindow window)
    {
        for (int y = 0; y < size.Height; y++)
        {
            for (int x = 0; x < size.Width; x++)
            {
                var Border = new Border()
                {
                    BorderThickness = new Thickness(
                        x == 0 ? tileGridBorderThickness : 0d,
                        y == 0 ? tileGridBorderThickness : 0d,
                        tileGridBorderThickness,
                        tileGridBorderThickness),
                    Background = new SolidColorBrush(
                        tile.Pixels[y * size.Width + x] ? Colors.Black : Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Gray)
                };

                window.TileGrid.Children.Add(Border);
            }
        }
    }

    private void Help()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "https://youtu.be/m5sbRiwQPMQ?t=87",
            UseShellExecute = true
        });
    }

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
        OnPropertyChanged(nameof(HexAngleText));
    }
}
