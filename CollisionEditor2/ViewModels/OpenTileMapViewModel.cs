using CollisionEditor2.ViewServices;
using System;
using System.Collections;
using System.ComponentModel;
using Avalonia.Controls;
using CollisionEditor2.Views;
using CollisionEditor2.Models;
using MessageBoxSlim.Avalonia.DTO;
using MessageBoxSlim.Avalonia.Enums;
using MessageBoxSlim.Avalonia;
using ReactiveUI;
using System.Reactive;

namespace CollisionEditor2.ViewModels
{
    public class OpenTileMapViewModel: ViewModelBase, INotifyDataErrorInfo
    {

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public string TileHeightText
        {
            get => tileHeight.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(TileHeightText));
                CheckErrors();
                int intTileHeight;
                bool isNumber = int.TryParse(value, out intTileHeight);

                if (isNumber && intTileHeight<4)
                {
                    textboxValidator.AddError(nameof(TileHeightText), "Wrong Tile Height!");
                    CheckErrors();
                    return;
                }
                
                tileHeight = intTileHeight;
            }
        }

        public string TileWidthText
        {
            get => tileWidth.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(TileWidthText));
                CheckErrors();
                int intTileWidth;
                bool isNumber = int.TryParse(value, out intTileWidth);

                if (isNumber && intTileWidth < 4)
                {
                    textboxValidator.AddError(nameof(TileWidthText), "Wrong Tile Width!");
                    CheckErrors();
                    return;
                }

                tileWidth = intTileWidth;
            }
        }
        public string VerticalSeparationText
        {
            get => verticalSeparation.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(VerticalSeparationText));
                CheckErrors();
                int intVerticalSeparation;
                bool isNumber = int.TryParse(value, out intVerticalSeparation);

                if (isNumber && intVerticalSeparation <0)
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
                int intHorizontalSeparation;
                bool isNumber = int.TryParse(value, out intHorizontalSeparation);

                if (isNumber && intHorizontalSeparation < 0)
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
                int intVerticalOffset;
                bool isNumber = int.TryParse(value, out intVerticalOffset);

                if (isNumber && intVerticalOffset <0)
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
                int intHorizontalOffset;
                bool isNumber = int.TryParse(value, out intHorizontalOffset);

                if (isNumber && intHorizontalOffset <0)
                {
                    textboxValidator.AddError(nameof(HorizontalOffsetText), "Wrong Horizontal Offset!");
                    CheckErrors();
                    return;
                }

                horizontalOffset = intHorizontalOffset;
            }
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


        private int tileHeight;
        private int tileWidth;
        private int verticalSeparation;
        private int horizontalSeparation;
        private int verticalOffset;
        private int horizontalOffset;

        private readonly TextboxValidator textboxValidator;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public OpenTileMap window;
        public OpenTileMapViewModel(OpenTileMap window)
        {
            textboxValidator = new TextboxValidator();
            textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
            SaveCommand = ReactiveCommand.Create(Save);

            this.window = window;
            tileHeight = 4;
            tileWidth = 4;
        }

        private void Save()
        {
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
            OnPropertyChanged(nameof(tileHeight));
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            return textboxValidator.GetErrors(propertyName);
        }

        public bool HasErrors => textboxValidator.HasErrors;
    }
}
