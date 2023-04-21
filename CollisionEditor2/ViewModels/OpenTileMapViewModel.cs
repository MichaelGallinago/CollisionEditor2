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
using CollisionEditor2.Models.ForAvalonia;

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
            get => tileHeight.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(TileHeightText));
                CheckErrors();
                bool isNumber = int.TryParse(value, out int intTileHeight);

                if (isNumber && intTileHeight< minTileHeight)
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
                bool isNumber = int.TryParse(value, out int intTileWidth);

                if (isNumber && intTileWidth < minTileWidth)
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
                bool isNumber = int.TryParse(value, out int intVerticalSeparation);

                if (isNumber && intVerticalSeparation < 0)
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
                bool isNumber = int.TryParse(value, out int intVerticalOffset);

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
                bool isNumber = int.TryParse(value, out int intHorizontalOffset);

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
        public OpenTileMapViewModel(OpenTileMap window,string filepath)
        {
            textboxValidator = new TextboxValidator();
            textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;
            SaveCommand = ReactiveCommand.Create(Save);

            this.window = window;
            tileHeight = minTileHeight;
            tileWidth = minTileWidth;
            
            window.ImageFromFile.Source = ViewModelAssistant.GetBitmap(filepath);
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

            OurMessageBox(tileWidth+" "+ tileHeight + " " + horizontalSeparation + " " + verticalSeparation + " " +
                          horizontalOffset + " " + verticalOffset);

            //window.Close();
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
