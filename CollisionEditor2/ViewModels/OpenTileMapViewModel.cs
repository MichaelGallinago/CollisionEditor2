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

    private readonly ValidatedPixelSize Separation = new(0, 0);
    private readonly ValidatedPixelSize Offset = new(0, 0);
    private readonly ValidatedPixelSize Size = new(16, 16);

    private readonly OpenTileMap window;
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
        get => Size.WidthString;
        set
        {
            Size.WidthString = value;
            HorizontalSet();
        }
    }

    public string TileHeightText
    {
        get => Size.HeightString;
        set
        {
            Size.HeightString = value;
            VerticalSet();
        }
    }

    public string HorizontalSeparationText
    {
        get => Separation.WidthString;
        set
        {
            Separation.WidthString = value;
            HorizontalSet();
        }
    }

    public string VerticalSeparationText
    {
        get => Separation.HeightString;
        set
        {
            Separation.HeightString = value;
            VerticalSet();
        }
    }

    public string HorizontalOffsetText
    {
        get => Offset.WidthString;
        set
        {
            Offset.WidthString = value;
            HorizontalSet();
        }
    }

    public string VerticalOffsetText
    {
        get => Offset.HeightString;
        set
        {
            Offset.HeightString = value;
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

        Size.Height = intTileHeight;
    }

    private void VerticalSeparationSet()
    {
        textboxValidator.ClearErrors(nameof(VerticalSeparationText));
        CheckErrors();

        bool isNumberVerticalSeparation = int.TryParse(VerticalSeparationText, out int intVerticalSeparation);

        if (!isNumberVerticalSeparation || intVerticalSeparation < 0 || bitmapSize.Height < Size.Height + intVerticalSeparation)
        {
            textboxValidator.AddError(nameof(VerticalSeparationText), "Wrong Vertical Separation!");
            CheckErrors();
            return;
        }

        Separation.Height = intVerticalSeparation;
    }
    private void VerticalOffsetSet()
    {
        textboxValidator.ClearErrors(nameof(VerticalOffsetText));
        CheckErrors();

        bool isNumberVerticalOffset = int.TryParse(VerticalOffsetText, out int intVerticalOffset);

        if (!isNumberVerticalOffset || intVerticalOffset < 0 || bitmapSize.Height < Size.Height + intVerticalOffset + Separation.Height)
        {
            textboxValidator.AddError(nameof(VerticalOffsetText), "Wrong Vertical Offset!");
            CheckErrors();
            return;
        }

        Offset.Height = intVerticalOffset;
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

        Size.Width = intTileWidth;
    }
    private void HorizontalSeparationSet()
    {
        textboxValidator.ClearErrors(nameof(HorizontalSeparationText));
        CheckErrors();
        bool isNumberHorizontalSeparation = int.TryParse(HorizontalSeparationText, out int intHorizontalSeparation);

        if (!isNumberHorizontalSeparation || intHorizontalSeparation < 0 || bitmapSize.Width < Size.Width + intHorizontalSeparation)
        {
            textboxValidator.AddError(nameof(HorizontalSeparationText), "Wrong Horizontal Separation!");
            CheckErrors();
            return;
        }

        Separation.Width = intHorizontalSeparation;
    }
    private void HorizontalOffsetSet()
    {
        textboxValidator.ClearErrors(nameof(HorizontalOffsetText));
        CheckErrors();
        bool isNumberHorizontalOffset = int.TryParse(HorizontalOffsetText, out int intHorizontalOffset);

        if (!isNumberHorizontalOffset || intHorizontalOffset < 0 || bitmapSize.Width < Size.Width + intHorizontalOffset + Separation.Width)
        {
            textboxValidator.AddError(nameof(HorizontalOffsetText), "Wrong Horizontal Offset!");
            CheckErrors();
            return;
        }

        Offset.Width = intHorizontalOffset;
    }

    private void CheckErrors()
    {
        window.OpenButton.IsEnabled = !textboxValidator.HasErrors;
    }

    private void Open()
    {
        window.IsOpened = true;

        window.HorizontalSeparation = Separation.Width;
        window.VerticalSeparation = Separation.Height;
        window.HorizontalOffset = Offset.Width;
        window.VerticalOffset = Offset.Height;
        window.TileWidth = Size.Width;
        window.TileHeight = Size.Height;

        window.Close();
    }

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
    }
}
