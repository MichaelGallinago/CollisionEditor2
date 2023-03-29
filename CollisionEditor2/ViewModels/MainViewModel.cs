using CollisionEditor2.Models;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.IO;
using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using CollisionEditor2.ViewServices;
using CollisionEditor2.Views;
using MessageBoxSlim.Avalonia.Enums;
using MessageBoxSlim.Avalonia.DTO;
using MessageBoxSlim.Avalonia;
using System.Diagnostics;
using ReactiveUI;
using System.Reactive;

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
    public ReactiveCommand<Unit, Unit> ExitAppCommand { get; }
    public ReactiveCommand<Unit, Unit> HelpCommand { get; }

    public byte ByteAngle
    {
        get => byteAngle;
        set
        {   
            byteAngle = value;
            ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

            window.DrawRedLine();
        }
    }

    public string HexAngle
    {
        get => hexAngle;
        set
        {
            hexAngle = value;

            textboxValidator.ClearErrors(nameof(HexAngle));
            if (hexAngle.Length != 4 || hexAngle[0] != '0' || hexAngle[1] != 'x'
                || !hexadecimalAlphabet.Contains(hexAngle[2]) || !hexadecimalAlphabet.Contains(hexAngle[3]))
            {
                textboxValidator.AddError(nameof(HexAngle), "Error! Wrong hexadecimal number");
                return;
            }

            Angles angles = ViewModelAngleService.GetAngles(hexAngle);
            ByteAngle = angles.ByteAngle;
            AngleMap.SetAngle((int)ChosenTile, angles.ByteAngle);
            window.DrawRedLine();
        }
    }

    public uint ChosenTile
    {
        get => chosenTile;
        set
        {   
            chosenTile = value;
        }
    }

    private const string hexadecimalAlphabet = "0123456789ABCDEF";
    private const int tileMapSeparation = 4;
    private const int tileMapTileScale = 2;

    public MainWindow window;
    private byte byteAngle;
    private string hexAngle;
    private uint chosenTile;
    private readonly TextboxValidator textboxValidator;

    public MainViewModel(MainWindow window)
    {
        textboxValidator = new TextboxValidator();
        textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
        AngleMap = new AngleMap(0);
        TileSet  = new TileSet(0);
        chosenTile = 0;
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

        ExitAppCommand = ReactiveCommand.Create(ExitApp);

        HelpCommand = ReactiveCommand.Create(Help);

        RectanglesGridUpdate();
        TileGridUpdate(TileSet, (int)chosenTile, window);
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

        ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        window.SelectTileTextBox.IsEnabled = true;
        window.SelectTileButton.IsEnabled  = true;

        TileMapGridUpdate(TileSet.Tiles.Count);
        window.DrawRedLine();
        SelectTile();
    }

    public void ShowAngles(Angles angles)
    {
        window.TextBoxByteAngle.IsEnabled = true;
        window.TextBoxHexAngle.IsEnabled = true;
        window.BorderFullAngle.BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(84, 84, 84));
        window.TextBlockFullAngle.Foreground = new SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 0, 0));
        window.TextBlockFullAngle.Background = new SolidColorBrush(Avalonia.Media.Color.FromRgb(177, 177, 177));

        byteAngle = angles.ByteAngle;
        OnPropertyChanged(nameof(ByteAngle));
        window.ByteAngleIncrimentButton.IsEnabled = true;
        window.ByteAngleDecrementButton.IsEnabled = true;

        hexAngle = angles.HexAngle;
        OnPropertyChanged(nameof(HexAngle));
        window.HexAngleIncrimentButton.IsEnabled = true;
        window.HexAngleDecrementButton.IsEnabled = true;

        window.TextBlockFullAngle.Text = angles.FullAngle.ToString() + "°";
    }

    private async void MenuOpenTileMap()
    {
        string filePath = await ViewModelFileService.GetFileOpenPath(window, ViewModelFileService.Filters.TileMap);
        if (filePath == string.Empty) 
        { 
            return; 
        }

        TileSet = new TileSet(filePath);
        AngleMap ??= new AngleMap(TileSet.Tiles.Count);

        ViewModelAssistant.SupplementElements(AngleMap,TileSet);
        ViewModelAssistant.BitmapConvert(TileSet.Tiles[(int)chosenTile]);

        TileGridUpdate(TileSet, (int)ChosenTile, window);
        RectanglesGridUpdate();
            
        window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
        window.Widths.Text  = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);
            
        ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        

        window.SelectTileTextBox.IsEnabled = true;
        window.SelectTileButton.IsEnabled  = true;


        TileMapGridReset();
        TileMapGridUpdate(TileSet.Tiles.Count);
        window.DrawRedLine();
        SelectTile();
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
        window.TileMapGrid.Height = (int)Math.Ceiling((double)tileCount / window.TileMapGrid.Columns) * (TileSet.TileSize.Height * tileMapTileScale + tileMapSeparation);
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
        }).ShowDialogAsync(window);
    }

    private async void MenuSaveTileMap()
    {
        if (TileSet.Tiles.Count == 0)
        {
            OurMessageBox("Error: You haven't chosen TileMap to save");
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
        if (TileSet.Tiles.Count == 0)
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
        if (TileSet.Tiles.Count == 0)
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
        if (AngleMap.Values.Count == 0)
        {
            OurMessageBox("Error: You haven't chosen AngleMap to save");
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

        TileGridUpdate(TileSet, (int)ChosenTile, window);
        window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
        window.Widths.Text = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

        ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
    }

    private void MenuUnloadAngleMap()
    {
        AngleMap = new AngleMap(TileSet.Tiles.Count);
        ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));
        window.RectanglesGrid.Children.Clear();
        window.DrawRedLine();
    }

    private void MenuUnloadAll()
    {
        window.TileMapGrid.Children.Clear();
        TileSet = new TileSet(0);
        AngleMap = new AngleMap(0);

        window.Heights.Text = null;
        window.Widths.Text = null;
        ShowAngles(new Angles(0, "0x00", 0));
        window.SelectTileTextBox.Text = "0";

        window.ByteAngleIncrimentButton.IsEnabled = false;
        window.ByteAngleDecrementButton.IsEnabled = false;
        window.HexAngleIncrimentButton.IsEnabled  = false;
        window.HexAngleDecrementButton.IsEnabled  = false;

        window.SelectTileTextBox.IsEnabled = false;
        window.SelectTileButton.IsEnabled  = false;
        window.TextBoxByteAngle.IsEnabled  = false;
        window.TextBoxHexAngle.IsEnabled   = false;

        //window.BorderFullAngle.BorderBrush = 


        window.canvasForLine.Children.Clear();
        window.RectanglesGrid.Children.Clear();

        TileMapGridUpdate(TileSet.Tiles.Count);
        TileGridUpdate(TileSet, 0, window);
    }

    private void AngleIncrement()
    {
        byte byteAngle = AngleMap.ChangeAngle((int)chosenTile, 1);

        ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

        window.DrawRedLine();
    }

    private void AngleDecrement()
    {
        byte byteAngle = AngleMap.ChangeAngle((int)chosenTile, -1);

        ShowAngles(ViewModelAngleService.GetAngles(byteAngle));

        window.DrawRedLine();
    }

    public void SelectTile()
    {
        if (chosenTile > TileSet.Tiles.Count - 1)
        {
            chosenTile = (uint)TileSet.Tiles.Count - 1;
            OnPropertyChanged(nameof(ChosenTile));
        }
        
        Border lastTile = GetTile(window.LastChosenTile);

        window.TileMapGrid.Children.RemoveAt(window.LastChosenTile);
        window.TileMapGrid.Children.Insert(window.LastChosenTile, lastTile);

        Border currentTile = GetTile((int)chosenTile);
        currentTile.BorderBrush = new SolidColorBrush(Colors.Red);

        window.TileMapGrid.Children.RemoveAt((int)chosenTile);
        window.TileMapGrid.Children.Insert((int)chosenTile, currentTile);

        window.LastChosenTile = (int)chosenTile;
        TileGridUpdate(TileSet, (int)ChosenTile, window);
        window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
        window.Widths.Text  = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);
        
        ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));

        window.DrawRedLine();
        window.RectanglesGrid.Children.Clear();
    }

    public void SelectTileFromTileMap()
    {
        OnPropertyChanged(nameof(ChosenTile));
        TileGridUpdate(TileSet, (int)ChosenTile, window);
        window.Heights.Text = ViewModelAssistant.GetCollisionValues(TileSet.HeightMap[(int)chosenTile]);
        window.Widths.Text = ViewModelAssistant.GetCollisionValues(TileSet.WidthMap[(int)chosenTile]);

        ShowAngles(ViewModelAssistant.GetAngles(AngleMap, chosenTile));

        window.DrawRedLine();
        window.RectanglesGrid.Children.Clear();
    }

    internal Border GetTile(int index)
    {
        Bitmap tile = TileSet.Tiles[index];

        var image = new Avalonia.Controls.Image
        {
            Width  = TileSet.TileSize.Width * 2,
            Height = TileSet.TileSize.Height * 2,
            Source = ViewModelAssistant.BitmapConvert(tile)
        };

        var border = new Border()
        {
            Background = new SolidColorBrush(Avalonia.Media.Color.FromRgb(230, 230, 230)),
            BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(211, 211, 211)),
            BorderThickness = new Thickness(tileMapSeparation / 2d),
            Child = image
        };

        return border;
    }

    private void ExitApp()
    {
        window.Close();
    }

    public void UpdateAngles(Vector2<int> positionGreen, Vector2<int> positionBlue)
    {
        if (AngleMap.Values.Count == 0)
        {
            return;
        }

        byte byteAngle = AngleMap.SetAngleWithLine((int)chosenTile, positionGreen, positionBlue);

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
                    BorderThickness = new Thickness(x == 0 ? 1d : 0d, y == 0 ? 1d : 0d, 1d, 1d),
                    Background = new SolidColorBrush(tile.GetPixel(x, y).A > 0 ? Colors.Black : Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Gray),
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

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
        OnPropertyChanged(nameof(HexAngle));
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return textboxValidator.GetErrors(propertyName);
    }

    public bool HasErrors => textboxValidator.HasErrors;
}
