using CollisionEditor2.ViewServices;
using CollisionEditor2.Views;
using MessageBoxSlim.Avalonia.Enums;
using MessageBoxSlim.Avalonia.DTO;
using MessageBoxSlim.Avalonia;
using System.ComponentModel;
using System.Collections;
using System.Reactive;
using System.Drawing;
using System;
using Avalonia.Controls;
using ReactiveUI;
using CollisionEditor2.Models.ForAvalonia;
using System.Security.Cryptography;

namespace CollisionEditor2.ViewModels
{
    public class OpenTileMapViewModel: ViewModelBase, INotifyDataErrorInfo
    {
        //Glory to RuChat!!!!!
        private const int minTileHeight = 4;
        private const int minTileWidth  = 4;

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public string TileHeightText
        {
            get => tileHeightString;
            set
            {
                tileHeightString = value;
                VerticalSet();
            }
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
        public string VerticalSeparationText
        {
            get => verticalSeparationString;
            set
            {
                verticalSeparationString = value;
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

        public string VerticalOffsetText
        {
            get => verticalOffsetString;
            set
            {
                verticalOffsetString = value;
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

        private void VerticalSet()
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

        private void HorizontalSet()
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
            if (textboxValidator.HasErrors)
            {
                window.SaveButton.IsEnabled = false;
            }
            else
            {
                window.SaveButton.IsEnabled = true;
            }
        }


        private int tileHeight=16;
        private int tileWidth =16;
        private int verticalSeparation;
        private int horizontalSeparation;
        private int verticalOffset;
        private int horizontalOffset;

        private string tileHeightString           = "16";
        private string tileWidthString            = "16";
        private string verticalSeparationString   = "0";
        private string horizontalSeparationString = "0";
        private string verticalOffsetString       = "0";
        private string horizontalOffsetString     = "0";

        private Size bitmapSize;

        private readonly TextboxValidator textboxValidator;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public OpenTileMap window;
        public OpenTileMapViewModel(OpenTileMap window,string filepath)
        {
            textboxValidator = new TextboxValidator();
            textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
            SaveCommand = ReactiveCommand.Create(Save);

            this.window = window;

            window.ImageFromFile.Source = ViewModelAssistant.GetBitmap(filepath, out Size bitmapSize);
            this.bitmapSize = bitmapSize;
        }

        private void Save()
        {
            window.IsSaved=true;

            window.TileHeight = tileHeight;
            window.TileWidth = tileWidth;
            window.VerticalSeparation = verticalSeparation;
            window.HorizontalSeparation = horizontalSeparation;
            window.VerticalOffset = verticalOffset;
            window.HorizontalOffset = horizontalOffset;
            
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
}
