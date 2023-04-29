using CollisionEditor2.ViewServices;
using CollisionEditor2.Views;
using MessageBoxSlim.Avalonia.Enums;
using MessageBoxSlim.Avalonia.DTO;
using MessageBoxSlim.Avalonia;
using System.ComponentModel;
using System.Collections;
using System.Reactive;
using System;
using Avalonia.Controls;
using ReactiveUI;
using Avalonia;
using CollisionEditor2.Models.ForAvalonia;
using CollisionEditor2.Models;
using SkiaSharp;

namespace CollisionEditor2.ViewModels;

public class SaveTileMapViewModel : ViewModelBase, INotifyDataErrorInfo
{
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateColorsCommand { get; }
    
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
        get => redChannel1.ToString();
        set
        {
            ByteValidation(value, nameof(RedChannel1Text),ref redChannel1);   
        }
    }
    public string GreenChannel1Text
    {
        get => greenChannel1.ToString();
        set
        {
            ByteValidation(value, nameof(GreenChannel1Text), ref greenChannel1);
        }
    }
    public string BlueChannel1Text
    {
        get => blueChannel1.ToString();
        set
        {
            ByteValidation(value, nameof(BlueChannel1Text), ref blueChannel1);
        }
    }
    public string AlphaChannel1Text
    {
        get => alphaChannel1.ToString();
        set
        {
            ByteValidation(value, nameof(AlphaChannel1Text), ref alphaChannel1);
        }
    }
    public string OffsetInTiles1Text
    {
        get => offsetInTiles1.ToString();
        set
        {
            OffsetValidation(value, nameof(OffsetInTiles1Text), ref offsetInTiles1);
        }
    }

    public string RedChannel2Text
    {
        get => redChannel2.ToString();
        set
        {
            ByteValidation(value, nameof(RedChannel2Text), ref redChannel2);
        }
    }
    public string GreenChannel2Text
    {
        get => greenChannel2.ToString();
        set
        {
            ByteValidation(value, nameof(GreenChannel2Text), ref greenChannel2);
        }
    }
    public string BlueChannel2Text
    {
        get => blueChannel2.ToString();
        set
        {
            ByteValidation(value, nameof(BlueChannel2Text), ref blueChannel2);
        }
    }
    public string AlphaChannel2Text
    {
        get => alphaChannel2.ToString();
        set
        {
            ByteValidation(value, nameof(AlphaChannel2Text), ref alphaChannel2);
        }
    }
    public string OffsetInTiles2Text
    {
        get => offsetInTiles2.ToString();
        set
        {
            OffsetValidation(value, nameof(OffsetInTiles2Text), ref offsetInTiles2);
        }
    }

    public string RedChannel3Text
    {
        get => redChannel3.ToString();
        set
        {
            ByteValidation(value, nameof(RedChannel3Text), ref redChannel3);
        }
    }
    public string GreenChannel3Text
    {
        get => greenChannel3.ToString();
        set
        {
            ByteValidation(value, nameof(GreenChannel3Text), ref greenChannel3);
        }
    }
    public string BlueChannel3Text
    {
        get => blueChannel3.ToString();
        set
        {
            ByteValidation(value, nameof(BlueChannel3Text), ref blueChannel3);
        }
    }
    public string AlphaChannel3Text
    {
        get => alphaChannel3.ToString();
        set
        {
            ByteValidation(value, nameof(AlphaChannel3Text), ref alphaChannel3);
        }
    }

    public string OffsetInTiles3Text
    {
        get => offsetInTiles3.ToString();
        set
        {
            OffsetValidation(value, nameof(OffsetInTiles3Text), ref offsetInTiles3);
        }
    }

    private void ByteValidation(string value, string nameColorChannel, ref byte resultColorChannel)
    {
        textboxValidator.ClearErrors(nameColorChannel);
        CheckErrors();
        bool isByte = byte.TryParse(value, out byte intColorChannel);

        if (!isByte)
        {
            textboxValidator.AddError(nameColorChannel, "Not a Byte!");
            CheckErrors();
            return;
        }

        resultColorChannel = intColorChannel;
    }
    private void OffsetValidation(string value, string nameOffsetInTiles, ref int resultoffsetInTiles)
    {
        textboxValidator.ClearErrors(nameOffsetInTiles);
        CheckErrors();
        bool isNumber = int.TryParse(value, out int intOffsetInTiles);

        if (!isNumber || intOffsetInTiles < 0 || intOffsetInTiles > 255)
        {
            textboxValidator.AddError(nameOffsetInTiles, "Not a Byte!");
            CheckErrors();
            return;
        }

        resultoffsetInTiles = intOffsetInTiles;
    }

    private void CheckErrors()
    {
        if (textboxValidator.HasErrors)
        {   
            window.SaveButton.IsEnabled = false;
            window.UpdateColorsButton.IsEnabled = false;
        }
        else
        {
            window.SaveButton.IsEnabled = true;
            window.UpdateColorsButton.IsEnabled = true;
        }
    }


    private int verticalSeparation;
    private int horizontalSeparation;
    private int verticalOffset;
    private int horizontalOffset;
    private int amountOfColumns = 8;

    //0 0 0 255
    //255 255 255 255
    //255 255 0 255

    private byte redChannel1    = 0;
    private byte greenChannel1  = 0;
    private byte blueChannel1   = 0;
    private byte alphaChannel1  = 255;
    private int  offsetInTiles1 = 0;

    private byte redChannel2    = 255;
    private byte greenChannel2  = 255;
    private byte blueChannel2   = 255;
    private byte alphaChannel2  = 255;
    private int  offsetInTiles2 = 0;

    private byte redChannel3    = 255;
    private byte greenChannel3  = 255;
    private byte blueChannel3   = 0;
    private byte alphaChannel3  = 255;
    private int  offsetInTiles3 = 0;

    private readonly TextboxValidator textboxValidator;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    private SaveTileMap window;
    private TileSet tileSet;
    private SKBitmap ?saveImage;

    public SaveTileMapViewModel(SaveTileMap window,TileSet tileSet)
    {
        textboxValidator = new TextboxValidator();
        textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;

        UpdateColorsCommand = ReactiveCommand.Create(UpdateColors);
        SaveCommand         = ReactiveCommand.Create(Save);

        this.tileSet = tileSet;
        this.window = window;

        UpdateColors();
    }

    private void UpdateColors()
    {
        OurColor[] ourColors = new OurColor[] { new OurColor(redChannel1, greenChannel1, blueChannel1, alphaChannel1),
                                                new OurColor(redChannel2, greenChannel2, blueChannel2, alphaChannel2),
                                                new OurColor(redChannel3, greenChannel3, blueChannel3, alphaChannel3)};

        saveImage = tileSet.DrawTileMap(amountOfColumns, ourColors,
                                        new int[] { offsetInTiles1, offsetInTiles2, offsetInTiles3 },
                                        new PixelSize(horizontalSeparation, verticalSeparation),
                                        new PixelSize(horizontalOffset, verticalOffset));

        window.SaveImage.Source = ViewModelAssistant.GetBitmapFromPixelArray(ViewModelAssistant.SKBitmapToPixelArray(saveImage),
                                                                            new PixelSize(saveImage.Width, saveImage.Height));
        window.SaveImageBorder.Height = saveImage.Height;
        window.SaveImageBorder.Width= saveImage.Width;
    }

    private void Save()
    {   
        window.IsSaved = true;
        window.ResultSaveImage = saveImage;

        window.Close();
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

    private void TextboxValidator_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return textboxValidator.GetErrors(propertyName);
    }

    public bool HasErrors => textboxValidator.HasErrors;
}
