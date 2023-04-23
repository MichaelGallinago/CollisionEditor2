using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.Models;
using CollisionEditor2.ViewServices;
using CollisionEditor2.Views;
using MessageBoxSlim.Avalonia.Enums;
using MessageBoxSlim.Avalonia.DTO;
using MessageBoxSlim.Avalonia;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.IO;
using System;
using System.Diagnostics;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using ReactiveUI;

namespace CollisionEditor2.ViewModels;

public class MainViewModel : ViewModelBase, INotifyDataErrorInfo
{   
    public AngleMap AngleMap { get; private set; }
    public TileSet TileSet { get; private set; }
    
    public ReactiveCommand<Unit, Unit> MenuOpenAngleMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuOpenTileMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuSaveTileMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuSaveWidthMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuSaveHeightMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuSaveAngleMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuSaveAllCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuUnloadTileMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuUnloadAngleMapCommand { get; }
    public ReactiveCommand<Unit, Unit> MenuUnloadAllCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectTileCommand { get; }
    public ReactiveCommand<Unit, Unit> AngleIncrementCommand { get; }
    public ReactiveCommand<Unit, Unit> AngleDecrementCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteTileCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitAppCommand { get; }
    public ReactiveCommand<Unit, Unit> HelpCommand { get; }
    

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
            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

            window.DrawRedLine();
        }
    }

    public string HexAngleText
    {
        get => hexAngle;
        set
        {
            textboxValidator.ClearErrors(nameof(HexAngleText));



            string prefix = value[..AngleService.HexAnglePrefixLength];

            if (prefix != "0x" || prefix != "0X")
            {
                textboxValidator.AddError(nameof(HexAngleText), 
                    "Wrong hexadecimal prefix!\nMust be '0x' or '0X'");
                return;

            }
            else if (value.Length <= AngleService.HexAnglePrefixLength)

            {
                textboxValidator.AddError(nameof(HexAngleText), 
                    $"Wrong hexadecimal number length!\nMust be between {AngleService.HexAnglePrefixLength} and "
                    + $"{AngleService.HexAnglePrefixLength + AngleService.HexAngleMaxLength}");
                return;

            }
            else if (!int.TryParse(value[AngleService.HexAnglePrefixLength..], 
                System.Globalization.NumberStyles.HexNumber, null, out _))
            {
                textboxValidator.AddError(nameof(HexAngleText), 
                    "Wrong hexadecimal number alphabet!\nMust be '0123456789ABCDEFabcdef'");
                return;
            }

            hexAngle = value;

            Angles angles = ViewModelAngleService.GetAngles(hexAngle);

            ByteAngleText = angles.ByteAngle.ToString();
            AngleMap.SetAngle(int.Parse(SelectedTileText), angles.ByteAngle);
            window.DrawRedLine();
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

    public int SelectedTile { get; set; }

    public MainWindow window;

    private const int tileMapSeparation = 4;
    private const double tileGridBorderThickness = 1d;
    private const double thicknessToSeparationRatio = 2d;
    private byte byteAngle;
    private string hexAngle;
    private readonly TextboxValidator textboxValidator;

    public MainViewModel(MainWindow window)
    {
        textboxValidator = new TextboxValidator();

        textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;

        AngleMap = new AngleMap();
        TileSet  = new TileSet();

        byteAngle = 0;
        hexAngle = "0x00";
        this.window = window;

        MenuOpenAngleMapCommand = ReactiveCommand.Create(MenuOpenAngleMap);
        MenuOpenTileMapCommand  = ReactiveCommand.Create(MenuOpenTileMap);

        MenuSaveTileMapCommand   = ReactiveCommand.Create(MenuSaveTileMap);
        MenuSaveWidthMapCommand  = ReactiveCommand.Create(MenuSaveWidthMap);
        MenuSaveHeightMapCommand = ReactiveCommand.Create(MenuSaveHeightMap);
        MenuSaveAngleMapCommand  = ReactiveCommand.Create(MenuSaveAngleMap);
        MenuSaveAllCommand       = ReactiveCommand.Create(MenuSaveAll);

        MenuUnloadTileMapCommand  = ReactiveCommand.Create(MenuUnloadTileMap);
        MenuUnloadAngleMapCommand = ReactiveCommand.Create(MenuUnloadAngleMap);
        MenuUnloadAllCommand      = ReactiveCommand.Create(MenuUnloadAll);

        AngleIncrementCommand = ReactiveCommand.Create(AngleIncrement);
        AngleDecrementCommand = ReactiveCommand.Create(AngleDecrement);
        SelectTileCommand     = ReactiveCommand.Create(SelectTile);

        DeleteTileCommand     = ReactiveCommand.Create(DeleteTile);

        ExitAppCommand = ReactiveCommand.Create(ExitApp);

        HelpCommand = ReactiveCommand.Create(Help);

        RectanglesGridUpdate();
        TileGridUpdate(TileSet, SelectedTile, window);
    }

    private async void MenuOpenAngleMap()
    {
        string filePath = await ViewModelFileService.GetFileOpenPath(window,ViewModelFileService.Filters.AngleMap);
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

        ShowAngles(AngleService.GetAngles(AngleMap, SelectedTile));
        window.SelectTileTextBox.IsEnabled = true;
        window.SelectTileButton.IsEnabled  = true;
        window.ModSwitchButton.IsEnabled = true;
        window.DeleteTileButton.IsEnabled = true;

        TileMapGridUpdate(TileSet.Tiles.Count);
        window.DrawRedLine();
        SelectTile();
    }

    public void ShowAngles(Angles angles)
    {
        window.TextBoxByteAngle.IsEnabled = true;
        window.TextBoxHexAngle.IsEnabled  = true;

        window.BorderFullAngle.BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(84, 84, 84));
        window.TextBlockFullAngle.Foreground = new SolidColorBrush(Colors.Black);
        window.TextBlockFullAngle.Background = new SolidColorBrush(Avalonia.Media.Color.FromRgb(177, 177, 177));

        byteAngle = angles.ByteAngle;
        OnPropertyChanged(nameof(ByteAngleText));
        window.ByteAngleIncrimentButton.IsEnabled = true;
        window.ByteAngleDecrementButton.IsEnabled = true;

        hexAngle = angles.HexAngle;
        OnPropertyChanged(nameof(HexAngleText));
        window.HexAngleIncrimentButton.IsEnabled = true;
        window.HexAngleDecrementButton.IsEnabled = true;

        window.TextBlockFullAngle.Text = angles.FullAngle + "°";
    }

    private async void MenuOpenTileMap()
    {
        string filePath = await ViewModelFileService.GetFileOpenPath(window, ViewModelFileService.Filters.TileMap);
        if (filePath == string.Empty) 
        { 
            return; 
        }

        OpenTileMap openTileMap = new();
        openTileMap.DataContext = new OpenTileMapViewModel(openTileMap, filePath);
        await openTileMap.ShowDialog(window);
        if (!openTileMap.IsSaved)
        {
            return;
        }

        TileSet = new TileSet(filePath, openTileMap.TileWidth, openTileMap.TileHeight,
            new System.Drawing.Size(openTileMap.HorizontalSeparation,openTileMap.VerticalSeparation), 
            new System.Drawing.Size(openTileMap.HorizontalOffset, openTileMap.VerticalOffset));

        if (AngleMap.Values.Count <= 0)
        {
            AngleMap = new AngleMap(TileSet.Tiles.Count);
        }

        ViewModelAssistant.SupplementElements(AngleMap,TileSet);
        ViewModelAssistant.BitmapConvert(TileSet.Tiles[SelectedTile]);

        TileGridUpdate(TileSet, SelectedTile, window);
        RectanglesGridUpdate();
            
        window.Heights.Text = TileService.GetCollisionValues(TileSet.HeightMap[SelectedTile]);
        window.Widths.Text  = TileService.GetCollisionValues(TileSet.WidthMap[SelectedTile]);
            
        ShowAngles(AngleService.GetAngles(AngleMap, SelectedTile));
        

        window.SelectTileTextBox.IsEnabled = true;
        window.SelectTileButton.IsEnabled  = true;
        window.ModSwitchButton.IsEnabled = true;
        window.DeleteTileButton.IsEnabled = true;

        TileMapGridReset();
        TileMapGridUpdate(TileSet.Tiles.Count);
        window.DrawRedLine();
        SelectTile();
        window.WindowSizeChanged(new Avalonia.Size(window.Width, window.Height));
    }

    private void TileMapGridReset()
    {
        window.TileMapGrid.Children.Clear();

        for (int i = 0; i < TileSet.Tiles.Count; i++)
        {
            window.TileMapGrid.Children.Add(GetTile(i));
        }
    }

    public void TileMapGridUpdate(int tileCount)
    {
        window.TileMapGrid.Height = (int)Math.Ceiling((double)tileCount / window.TileMapGrid.Columns) 
            * (TileSet.TileSize.Height * MainWindow.TileMapTileScale + tileMapSeparation);
    }

    public async void OurMessageBox(string message)
    {
        _ = await BoxedMessage.Create(new MessageBoxParams
        {
            Buttons = ButtonEnum.Ok,
            ContentTitle = "Error",
            ContentMessage = message,
            Icon = new Avalonia.Media.Imaging.Bitmap("../../../../CollisionEditor2/Assets/avalonia-logo.ico"),
            Location = WindowStartupLocation.CenterScreen,
            CanResize = false,
        }).ShowDialogAsync(window);
    }

    private async void MenuSaveTileMap()
    {
        if (TileSet.Tiles.Count <= 0)
        {
            OurMessageBox("Error: You haven't selected TileMap to save");
            return;
        }
        SaveTileMap saveTileMap = new();
        saveTileMap.DataContext = new SaveTileMapViewModel(saveTileMap);
        await saveTileMap.ShowDialog(window);
        if (!saveTileMap.IsSaved)
        {
            return;
        }

        string filePath = await ViewModelFileService.GetFileSavePath(window, ViewModelFileService.Filters.TileMap);
        if (filePath != string.Empty)
        {
            TileSet.Save(Path.GetFullPath(filePath), 16);
        }
    }

    private async void MenuSaveWidthMap()
    {
        if (TileSet.Tiles.Count <= 0)
        {
            OurMessageBox("Error: The WidthMap isn't generated!");
            return;
        }

        string filePath = await ViewModelFileService.GetFileSavePath(window, ViewModelFileService.Filters.WidthMap);
        if (filePath != string.Empty)
        {
            TileSet.SaveCollisionMap(Path.GetFullPath(filePath), TileSet.WidthMap);
        }
    }

    private async void MenuSaveHeightMap()
    {
        if (TileSet.Tiles.Count <= 0)
        {
            
            OurMessageBox("Error: The HeightMap isn't generated!");
            return;
        }

        string filePath = await ViewModelFileService.GetFileSavePath(window, ViewModelFileService.Filters.HeightMap);
        if (filePath != string.Empty)
        {
            TileSet.SaveCollisionMap(Path.GetFullPath(filePath), TileSet.HeightMap);
        }
    }

    private async void MenuSaveAngleMap()
    {
        if (AngleMap.Values.Count <= 0)
        {
            OurMessageBox("Error: You haven't selected AngleMap to save");
            return;
        }

        string filePath = await ViewModelFileService.GetFileSavePath(window, ViewModelFileService.Filters.AngleMap);
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
        TileSet = new TileSet(AngleMap.Values.Count);
        TileMapGridReset();

        TileGridUpdate(TileSet, SelectedTile, window);
        window.Heights.Text = TileService.GetCollisionValues(TileSet.HeightMap[SelectedTile]);
        window.Widths.Text  = TileService.GetCollisionValues(TileSet.WidthMap[SelectedTile]);

        ShowAngles(AngleService.GetAngles(AngleMap, SelectedTile));
    }

    private void MenuUnloadAngleMap()
    {
        AngleMap = new AngleMap(TileSet.Tiles.Count);
        ShowAngles(AngleService.GetAngles(AngleMap, SelectedTile));
        window.RectanglesGrid.Children.Clear();
        window.DrawRedLine();
    }

    private void MenuUnloadAll()
    {
        window.TileMapGrid.Children.Clear();

        TileSet  = new TileSet();
        AngleMap = new AngleMap();

        window.Heights.Text = null;
        window.Widths.Text  = null;

        ShowAngles(new Angles(0, "0x00", 0));

        SelectedTile = 0;
        window.SelectTileTextBox.Text = "0";
        window.LastSelectedTile = 0;

        window.ByteAngleIncrimentButton.IsEnabled = false;
        window.ByteAngleDecrementButton.IsEnabled = false;
        window.HexAngleIncrimentButton.IsEnabled  = false;
        window.HexAngleDecrementButton.IsEnabled  = false;

        window.SelectTileTextBox.IsEnabled = false;
        window.SelectTileButton.IsEnabled  = false;
        window.ModSwitchButton.IsEnabled = false;
        window.DeleteTileButton.IsEnabled = false;

        window.TextBoxByteAngle.IsEnabled  = false;
        window.TextBoxHexAngle.IsEnabled   = false;

        window.canvasForLine.Children.Clear();
        window.RectanglesGrid.Children.Clear();

        TileMapGridUpdate(TileSet.Tiles.Count);
        TileGridUpdate(TileSet, 0, window);
    }

    private void AngleIncrement()
    {
        byte byteAngle = AngleMap.ChangeAngle(SelectedTile, 1);

        ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

        window.DrawRedLine();
    }

    private void AngleDecrement()
    {
        byte byteAngle = AngleMap.ChangeAngle(SelectedTile, -1);

        ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

        window.DrawRedLine();
    }

    public void SelectTile()
    {
        if (SelectedTile > TileSet.Tiles.Count - 1)
        {
            SelectedTile = TileSet.Tiles.Count - 1;
            OnPropertyChanged(nameof(SelectedTile));
        }
        
        //TODO
        //Border lastTile = GetTile(window.LastSelectedTile);

        ((Border)window.TileMapGrid.Children[window.LastSelectedTile]).BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(211, 211, 211));

        //window.TileMapGrid.Children.RemoveAt(window.LastSelectedTile);
        //window.TileMapGrid.Children.Insert(window.LastSelectedTile, lastTile);

        //Border currentTile = GetTile(SelectedTile);
        //currentTile.BorderBrush = new SolidColorBrush(Colors.Red);

        //window.TileMapGrid.Children.RemoveAt(SelectedTile);
        //window.TileMapGrid.Children.Insert(SelectedTile, currentTile);

        ((Border)window.TileMapGrid.Children[SelectedTile]).BorderBrush = new SolidColorBrush(Colors.Red);

        window.LastSelectedTile = SelectedTile;
        TileGridUpdate(TileSet, SelectedTile, window);
        window.Heights.Text = TileService.GetCollisionValues(TileSet.HeightMap[SelectedTile]);
        window.Widths.Text  = TileService.GetCollisionValues(TileSet.WidthMap[SelectedTile]);
        
        ShowAngles(AngleService.GetAngles(AngleMap, SelectedTile));

        window.DrawRedLine();
        window.RectanglesGrid.Children.Clear();
    }

    public void SelectTileFromTileMap()
    {
        OnPropertyChanged(nameof(SelectedTileText));
        TileGridUpdate(TileSet, SelectedTile, window);
        window.Heights.Text = TileService.GetCollisionValues(TileSet.HeightMap[SelectedTile]);
        window.Widths.Text  = TileService.GetCollisionValues(TileSet.WidthMap[SelectedTile]);

        ShowAngles(AngleService.GetAngles(AngleMap, SelectedTile));

        window.DrawRedLine();
        window.RectanglesGrid.Children.Clear();
    }

    public Border GetTile(int index)
    {
        Bitmap tile = TileSet.Tiles[index];

        var image = new Avalonia.Controls.Image
        {
            Width  = TileSet.TileSize.Width  * MainWindow.TileMapTileScale,
            Height = TileSet.TileSize.Height * MainWindow.TileMapTileScale,
            Source = ViewModelAssistant.BitmapConvert(tile)
        };

        var border = new Border()
        {
            Background  = new SolidColorBrush(Avalonia.Media.Color.FromRgb(230, 230, 230)),
            BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(211, 211, 211)),
            BorderThickness = new Thickness(tileMapSeparation / thicknessToSeparationRatio),
            Child = image
        };

        return border;
    }

    public void DeleteTile()
    {   
        
        TileSet.RemoveTile(SelectedTile);
        

        TileGridUpdate(TileSet, SelectedTile, window);
        window.Heights.Text = TileService.GetCollisionValues(TileSet.HeightMap[SelectedTile]);
        window.Widths.Text = TileService.GetCollisionValues(TileSet.WidthMap[SelectedTile]);
        TileMapGridReset();
    }

    private void ExitApp()
    {
        window.Close();
    }

    public void UpdateAngles(Vector2<int> positionGreen, Vector2<int> positionBlue)
    {
        if (AngleMap.Values.Count <= 0)
        {
            return;
        }

        byte byteAngle = AngleMap.SetAngleWithLine(SelectedTile, positionGreen, positionBlue);

        ShowAngles(ViewModelAngleService.GetAngles(byteAngle));
    }

    private void RectanglesGridUpdate()
    {
        window.RectanglesGrid.ColumnDefinitions.Clear();
        window.RectanglesGrid.RowDefinitions.Clear();

        var size = TileSet.TileSize;

        for (int x = 0; x < size.Width; x++)
        {
            window.RectanglesGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (int y = 0; y < size.Height; y++)
        {
            window.RectanglesGrid.RowDefinitions.Add(new RowDefinition());
        }
    }

    private static void TileGridUpdate(TileSet tileSet, int ChosenTile, MainWindow window)
    {
        window.TileGrid.Children.Clear();

        var size = tileSet.TileSize;

        window.TileGrid.Rows    = size.Height;
        window.TileGrid.Columns = size.Width;
        window.TileGrid.Background = new SolidColorBrush(Colors.Transparent);

        Bitmap tile = tileSet.Tiles.Count > 0 ? tileSet.Tiles[ChosenTile] : new Bitmap(size.Height, size.Width);

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

                    Background = new SolidColorBrush(tile.GetPixel(x, y).A > 0 ? Colors.Black : Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Gray),
                };

                window.TileGrid.Children.Add(Border);
            }
        }
    }

    public void EditTile(Vector2<int> tilePosition, bool isLeftButtonPressed)
    {
        TileSet.TileChangeLine(SelectedTile, tilePosition, isLeftButtonPressed);
        TileGridUpdate(TileSet, SelectedTile, window);
        window.Heights.Text = TileService.GetCollisionValues(TileSet.HeightMap[SelectedTile]);
        window.Widths.Text = TileService.GetCollisionValues(TileSet.WidthMap[SelectedTile]);
        Border newTile = GetTile(SelectedTile);
        newTile.BorderBrush = new SolidColorBrush(Colors.Red);
        window.TileMapGrid.Children[SelectedTile]=newTile;
    }

    private void Help()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "https://youtu.be/m5sbRiwQPMQ?t=87",
            UseShellExecute = true
        });
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
        OnPropertyChanged(nameof(HexAngleText));
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return textboxValidator.GetErrors(propertyName);
    }

    public bool HasErrors => textboxValidator.HasErrors;

}
