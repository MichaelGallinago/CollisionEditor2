using Avalonia;
using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.Views;
using CollisionEditor2.ViewUtilities;
using ReactiveUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reactive;

namespace CollisionEditor2.ViewModels;

public class OpenTileMapViewModel : ViewModelBase, INotifyDataErrorInfo
{
    //Glory to RuChat!!!!!
    private const int minTileHeight = 4;
    private const int minTileWidth = 4;

    private int horizontalSeparation;
    private int verticalSeparation;
    private int horizontalOffset;
    private int verticalOffset;
    private int tileWidth = 16;
    private int tileHeight = 16;

    private string horizontalSeparationString = "0";
    private string verticalSeparationString = "0";
    private string horizontalOffsetString = "0";
    private string verticalOffsetString = "0";
    private string tileWidthString = "16";
    private string tileHeightString = "16";

    private OpenTileMap window;
    private readonly PixelSize bitmapSize;

    private readonly TextboxValidator textboxValidator;

    public ReactiveCommand<Unit, Unit> OpenCommand { get; }
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public OpenTileMapViewModel(OpenTileMap window, string filePath)
    {
        textboxValidator = new TextboxValidator();
        textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
        OpenCommand = ReactiveCommand.Create(Open);

        this.window = window;

        window.ImageFromFile.Source = ViewModelAssistant.OpenBitmap(filePath, out PixelSize bitmapSize);

        window.ImageFromFileBorder.Height = bitmapSize.Height;
        window.ImageFromFileBorder.Width = bitmapSize.Width;

        this.bitmapSize = bitmapSize;
    }

    public string TileWidthText
    {
        get => tileWidthString;
        set
        {
            tileWidthString = value;
            HorizontalSet();
        }
    }

    public string TileHeightText
    {
        get => tileHeightString;
        set
        {
            tileHeightString = value;
            VerticalSet();
        }
    }

    public string HorizontalSeparationText
    {
        get => horizontalSeparationString;
        set
        {
            horizontalSeparationString = value;
            HorizontalSet();
        }
    }

    public string VerticalSeparationText
    {
        get => verticalSeparationString;
        set
        {
            verticalSeparationString = value;
            VerticalSet();
        }
    }

    public string HorizontalOffsetText
    {
        get => horizontalOffsetString;
        set
        {
            horizontalOffsetString = value;
            HorizontalSet();
        }
    }

    public string VerticalOffsetText
    {
        get => verticalOffsetString;
        set
        {
            verticalOffsetString = value;
            VerticalSet();
        }
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return textboxValidator.GetErrors(propertyName);
    }

    public bool HasErrors => textboxValidator.HasErrors;

    private void VerticalSet()
    {
        TileHeightSet();
        VerticalSeparationSet();
        VerticalOffsetSet();
    }
    private void HorizontalSet()
    {
        TileWidthSet();
        HorizontalSeparationSet();
        HorizontalOffsetSet();
    }
    private void TileHeightSet()
    {
        textboxValidator.ClearErrors(nameof(TileHeightText));
        CheckErrors();
        bool isNumberTileHeight = int.TryParse(TileHeightText, out int intTileHeight);

        if (!isNumberTileHeight || intTileHeight < minTileHeight || bitmapSize.Height < intTileHeight || intTileHeight > 32)
        {
            textboxValidator.AddError(nameof(TileHeightText), "Wrong Tile Height!");
            CheckErrors();
            return;
        }

        tileHeight = intTileHeight;
    }

    private void VerticalSeparationSet()
    {
        textboxValidator.ClearErrors(nameof(VerticalSeparationText));
        CheckErrors();

        bool isNumberVerticalSeparation = int.TryParse(VerticalSeparationText, out int intVerticalSeparation);

        if (!isNumberVerticalSeparation || intVerticalSeparation < 0 || bitmapSize.Height < tileHeight + intVerticalSeparation)
        {
            textboxValidator.AddError(nameof(VerticalSeparationText), "Wrong Vertical Separation!");
            CheckErrors();
            return;
        }

        verticalSeparation = intVerticalSeparation;
    }
    private void VerticalOffsetSet()
    {
        textboxValidator.ClearErrors(nameof(VerticalOffsetText));
        CheckErrors();

        bool isNumberVerticalOffset = int.TryParse(VerticalOffsetText, out int intVerticalOffset);

        if (!isNumberVerticalOffset || intVerticalOffset < 0 || bitmapSize.Height < tileHeight + intVerticalOffset + verticalSeparation)
        {
            textboxValidator.AddError(nameof(VerticalOffsetText), "Wrong Vertical Offset!");
            CheckErrors();
            return;
        }

        verticalOffset = intVerticalOffset;
    }
    private void TileWidthSet()
    {
        textboxValidator.ClearErrors(nameof(TileWidthText));
        CheckErrors();
        bool isNumberTileWidth = int.TryParse(TileWidthText, out int intTileWidth);

        if (!isNumberTileWidth || intTileWidth < minTileWidth || bitmapSize.Width < intTileWidth || intTileWidth > 32)
        {
            textboxValidator.AddError(nameof(TileWidthText), "Wrong Tile Width!");
            CheckErrors();
            return;
        }

        tileWidth = intTileWidth;
    }
    private void HorizontalSeparationSet()
    {
        textboxValidator.ClearErrors(nameof(HorizontalSeparationText));
        CheckErrors();
        bool isNumberHorizontalSeparation = int.TryParse(HorizontalSeparationText, out int intHorizontalSeparation);

        if (!isNumberHorizontalSeparation || intHorizontalSeparation < 0 || bitmapSize.Width < tileWidth + intHorizontalSeparation)
        {
            textboxValidator.AddError(nameof(HorizontalSeparationText), "Wrong Horizontal Separation!");
            CheckErrors();
            return;
        }

        horizontalSeparation = intHorizontalSeparation;
    }
    private void HorizontalOffsetSet()
    {
        textboxValidator.ClearErrors(nameof(HorizontalOffsetText));
        CheckErrors();
        bool isNumberHorizontalOffset = int.TryParse(HorizontalOffsetText, out int intHorizontalOffset);

        if (!isNumberHorizontalOffset || intHorizontalOffset < 0 || bitmapSize.Width < tileWidth + intHorizontalOffset + horizontalSeparation)
        {
            textboxValidator.AddError(nameof(HorizontalOffsetText), "Wrong Horizontal Offset!");
            CheckErrors();
            return;
        }

        horizontalOffset = intHorizontalOffset;
    }

    private void CheckErrors()
    {
        window.OpenButton.IsEnabled = !textboxValidator.HasErrors;
    }

    private void Open()
    {
        window.IsOpened = true;

        window.HorizontalSeparation = horizontalSeparation;
        window.VerticalSeparation = verticalSeparation;
        window.HorizontalOffset = horizontalOffset;
        window.VerticalOffset = verticalOffset;
        window.TileWidth = tileWidth;
        window.TileHeight = tileHeight;

        window.Close();
    }

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
    }
}
