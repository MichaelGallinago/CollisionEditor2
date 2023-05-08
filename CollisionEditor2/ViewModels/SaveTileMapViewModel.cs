using Avalonia;
using CollisionEditor2.Models;
using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.Views;
using CollisionEditor2.ViewUtilities;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reactive;

namespace CollisionEditor2.ViewModels;

public class SaveTileMapViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private int verticalSeparation;
    private int horizontalSeparation;
    private int verticalOffset;
    private int horizontalOffset;
    private int amountOfColumns = 8;

    private ColorGroup colorGroup1 = new ColorGroup(0, 0, 0, 255, 0);
    private ColorGroup colorGroup2 = new ColorGroup(255, 255, 255, 255, 0);
    private ColorGroup colorGroup3 = new ColorGroup(255, 255, 0, 255, 0);

    private SaveTileMap window;
    private TileSet tileSet;
    private SKBitmap? saveImage;

    private readonly TextboxValidator textboxValidator;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateColorsCommand { get; }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public SaveTileMapViewModel(SaveTileMap window, TileSet tileSet)
    {
        textboxValidator = new TextboxValidator();
        textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;

        UpdateColorsCommand = ReactiveCommand.Create(UpdateColors);
        SaveCommand = ReactiveCommand.Create(Save);

        this.tileSet = tileSet;
        this.window = window;

        UpdateColors();
    }
    public string HorizontalSeparationText
    {
        get => horizontalSeparation.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(HorizontalSeparationText));
            CheckErrors();
            bool isNumber = int.TryParse(value, out int intHorizontalSeparation);

            if (!isNumber || intHorizontalSeparation < 0)
            {
                textboxValidator.AddError(nameof(HorizontalSeparationText), "Wrong Horizontal Separation!");
                CheckErrors();
                return;
            }

            horizontalSeparation = intHorizontalSeparation;
        }
    }
    public string VerticalSeparationText
    {
        get => verticalSeparation.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(VerticalSeparationText));
            CheckErrors();
            bool isNumber = int.TryParse(value, out int intVerticalSeparation);

            if (!isNumber || intVerticalSeparation < 0)
            {
                textboxValidator.AddError(nameof(VerticalSeparationText), "Wrong Vertical Separation!");
                CheckErrors();
                return;
            }

            verticalSeparation = intVerticalSeparation;
        }
    }
    public string HorizontalOffsetText
    {
        get => horizontalOffset.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(HorizontalOffsetText));
            CheckErrors();
            bool isNumber = int.TryParse(value, out int intHorizontalOffset);

            if (!isNumber || intHorizontalOffset < 0)
            {
                textboxValidator.AddError(nameof(HorizontalOffsetText), "Wrong Horizontal Offset!");
                CheckErrors();
                return;
            }

            horizontalOffset = intHorizontalOffset;
        }
    }
    public string VerticalOffsetText
    {
        get => verticalOffset.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(VerticalOffsetText));
            CheckErrors();
            bool isNumber = int.TryParse(value, out int intVerticalOffset);

            if (!isNumber || intVerticalOffset < 0)
            {
                textboxValidator.AddError(nameof(VerticalOffsetText), "Wrong Vertical Offset!");
                CheckErrors();
                return;
            }

            verticalOffset = intVerticalOffset;
        }
    }

    public string AmountOfColumnsText
    {
        get => amountOfColumns.ToString();
        set
        {
            textboxValidator.ClearErrors(nameof(AmountOfColumnsText));
            CheckErrors();
            bool isNumber = int.TryParse(value, out int intAmountOfColumns);

            if (!isNumber || intAmountOfColumns <= 0)
            {
                textboxValidator.AddError(nameof(AmountOfColumnsText), "Wrong Amount of Columns!");
                CheckErrors();
                return;
            }

            amountOfColumns = intAmountOfColumns;
        }
    }

    public string RedChannel1Text
    {
        get => colorGroup1.RedChannel.ToString();
        set
        {
            colorGroup1.RedChannel = ByteValidation(value, nameof(RedChannel1Text), colorGroup1.RedChannel);
        }
    }
    public string GreenChannel1Text
    {
        get => colorGroup1.GreenChannel.ToString();
        set
        {
            colorGroup1.GreenChannel = ByteValidation(value, nameof(GreenChannel1Text), colorGroup1.GreenChannel);
        }
    }
    public string BlueChannel1Text
    {
        get => colorGroup1.BlueChannel.ToString();
        set
        {
            colorGroup1.BlueChannel = ByteValidation(value, nameof(BlueChannel1Text), colorGroup1.BlueChannel);
        }
    }
    public string AlphaChannel1Text
    {
        get => colorGroup1.AlphaChannel.ToString();
        set
        {
            colorGroup1.AlphaChannel = ByteValidation(value, nameof(AlphaChannel1Text), colorGroup1.AlphaChannel);
        }
    }
    public string OffsetInTiles1Text
    {
        get => colorGroup1.OffsetInTiles.ToString();
        set
        {
            colorGroup1.OffsetInTiles = OffsetValidation(value, nameof(OffsetInTiles1Text), colorGroup1.OffsetInTiles);
        }
    }

    public string RedChannel2Text
    {
        get => colorGroup2.RedChannel.ToString();
        set
        {
            colorGroup2.RedChannel = ByteValidation(value, nameof(RedChannel2Text), colorGroup2.RedChannel);
        }
    }
    public string GreenChannel2Text
    {
        get => colorGroup2.GreenChannel.ToString();
        set
        {
            colorGroup2.GreenChannel = ByteValidation(value, nameof(GreenChannel2Text), colorGroup2.GreenChannel);
        }
    }
    public string BlueChannel2Text
    {
        get => colorGroup2.BlueChannel.ToString();
        set
        {
            colorGroup2.BlueChannel = ByteValidation(value, nameof(BlueChannel2Text), colorGroup2.BlueChannel);
        }
    }
    public string AlphaChannel2Text
    {
        get => colorGroup2.AlphaChannel.ToString();
        set
        {
            colorGroup2.AlphaChannel = ByteValidation(value, nameof(AlphaChannel2Text), colorGroup2.AlphaChannel);
        }
    }
    public string OffsetInTiles2Text
    {
        get => colorGroup2.OffsetInTiles.ToString();
        set
        {
            colorGroup2.OffsetInTiles = OffsetValidation(value, nameof(OffsetInTiles2Text), colorGroup2.OffsetInTiles);
        }
    }

    public string RedChannel3Text
    {
        get => colorGroup3.RedChannel.ToString();
        set
        {
            colorGroup3.RedChannel = ByteValidation(value, nameof(RedChannel3Text), colorGroup3.RedChannel);
        }
    }
    public string GreenChannel3Text
    {
        get => colorGroup3.GreenChannel.ToString();
        set
        {
            colorGroup3.GreenChannel = ByteValidation(value, nameof(GreenChannel3Text), colorGroup3.GreenChannel);
        }
    }
    public string BlueChannel3Text
    {
        get => colorGroup3.BlueChannel.ToString();
        set
        {
            colorGroup3.BlueChannel = ByteValidation(value, nameof(BlueChannel3Text), colorGroup3.BlueChannel);
        }
    }
    public string AlphaChannel3Text
    {
        get => colorGroup3.AlphaChannel.ToString();
        set
        {
            colorGroup3.AlphaChannel = ByteValidation(value, nameof(AlphaChannel3Text), colorGroup3.AlphaChannel);
        }
    }

    public string OffsetInTiles3Text
    {
        get => colorGroup3.OffsetInTiles.ToString();
        set
        {
            colorGroup3.OffsetInTiles = OffsetValidation(value, nameof(OffsetInTiles3Text), colorGroup3.OffsetInTiles);
        }
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return textboxValidator.GetErrors(propertyName);
    }

    public bool HasErrors => textboxValidator.HasErrors;

    private byte ByteValidation(string value, string nameColorChannel, byte oldByteValueColorChannel)
    {
        textboxValidator.ClearErrors(nameColorChannel);
        CheckErrors();
        bool isByte = byte.TryParse(value, out byte newByteValueColorChannel);

        if (!isByte)
        {
            textboxValidator.AddError(nameColorChannel, "Not a Byte!");
            CheckErrors();
            return oldByteValueColorChannel;
        }

        return newByteValueColorChannel;
    }
    private int OffsetValidation(string value, string nameOffsetInTiles, int oldIntValueOffsetInTiles)
    {
        textboxValidator.ClearErrors(nameOffsetInTiles);
        CheckErrors();
        bool isNumber = int.TryParse(value, out int newIntOffsetInTiles);

        if (!isNumber || newIntOffsetInTiles < 0 || newIntOffsetInTiles > 255)
        {
            textboxValidator.AddError(nameOffsetInTiles, "Not a Byte!");
            CheckErrors();
            return oldIntValueOffsetInTiles;
        }

        return newIntOffsetInTiles;
    }

    private void CheckErrors()
    {
        window.SaveButton.IsEnabled = !textboxValidator.HasErrors;
        window.UpdateColorsButton.IsEnabled = !textboxValidator.HasErrors;
    }

    private void UpdateColors()
    {
        var ourColors = new OurColor[]
        {
            new OurColor(colorGroup1),
            new OurColor(colorGroup2),
            new OurColor(colorGroup3)
        };

        saveImage = tileSet.DrawTileMap(amountOfColumns, ourColors,
            new int[] { colorGroup1.OffsetInTiles, colorGroup2.OffsetInTiles, colorGroup3.OffsetInTiles },
            new PixelSize(horizontalSeparation, verticalSeparation),
            new PixelSize(horizontalOffset, verticalOffset));

        window.SaveImage.Source = ViewModelAssistant.GetBitmapFromPixelArray(
            ViewModelAssistant.GetPixelArrayFromSKBitmap(saveImage),
            new PixelSize(saveImage.Width, saveImage.Height));

        window.SaveImageBorder.Height = saveImage.Height;
        window.SaveImageBorder.Width = saveImage.Width;
    }

    private void Save()
    {
        window.IsSaved = true;
        window.ResultSaveImage = saveImage;

        window.Close();
    }

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
    }
}
