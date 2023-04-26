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
using System.Security.Cryptography;
using Avalonia.Media;
using Avalonia.Layout;
using System.Drawing.Printing;

namespace CollisionEditor2.ViewModels
{
    public class SaveTileMapViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        //Glory to RuChat!!!!!
        private const int minTileHeight = 4;
        private const int minTileWidth = 4;

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> AddGroupCommand { get; }
        
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

                if (!isNumber || intAmountOfColumns < 0)
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
                textboxValidator.ClearErrors(nameof(RedChannel1Text));
                CheckErrors();
                bool isNumber = int.TryParse(value, out int intRedChannel1);

                if (!isNumber || intRedChannel1 < 0|| intRedChannel1>255)
                {
                    textboxValidator.AddError(nameof(RedChannel1Text), "Not a Byte!");
                    CheckErrors();
                    return;
                }

                redChannel1 = intRedChannel1;
            }
        }
        public string GreenChannel1Text
        {
            get => greenChannel1.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(GreenChannel1Text));
                CheckErrors();
                bool isNumber = int.TryParse(value, out int intGreenChannel1);

                if (!isNumber || intGreenChannel1 < 0 || intGreenChannel1 > 255)
                {
                    textboxValidator.AddError(nameof(GreenChannel1Text), "Not a Byte!");
                    CheckErrors();
                    return;
                }

                greenChannel1 = intGreenChannel1;
            }
        }
        public string BlueChannel1Text
        {
            get => blueChannel1.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(BlueChannel1Text));
                CheckErrors();
                bool isNumber = int.TryParse(value, out int intBlueChannel1);

                if (!isNumber || intBlueChannel1 < 0 || intBlueChannel1 > 255)
                {
                    textboxValidator.AddError(nameof(BlueChannel1Text), "Not a Byte!");
                    CheckErrors();
                    return;
                }

                blueChannel1 = intBlueChannel1;
            }
        }

        public string AlphaChannel1Text
        {
            get => alphaChannel1.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(AlphaChannel1Text));
                CheckErrors();
                bool isNumber = int.TryParse(value, out int intAlphaChannel1);

                if (!isNumber || intAlphaChannel1 < 0 || intAlphaChannel1 > 255)
                {
                    textboxValidator.AddError(nameof(AlphaChannel1Text), "Not a Byte!");
                    CheckErrors();
                    return;
                }

                alphaChannel1 = intAlphaChannel1;
            }
        }

        public string OffsetInTiles1Text
        {
            get => offsetInTiles1.ToString();
            set
            {
                textboxValidator.ClearErrors(nameof(OffsetInTiles1Text));
                CheckErrors();
                bool isNumber = int.TryParse(value, out int intOffsetInTiles1);

                if (!isNumber || intOffsetInTiles1 < 0 || intOffsetInTiles1 > 255)
                {
                    textboxValidator.AddError(nameof(OffsetInTiles1Text), "Wrong Offset!");
                    CheckErrors();
                    return;
                }

                offsetInTiles1 = intOffsetInTiles1;
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


        private int verticalSeparation;
        private int horizontalSeparation;
        private int verticalOffset;
        private int horizontalOffset;
        private int amountOfColumns;

        private int redChannel1;
        private int greenChannel1;
        private int blueChannel1;
        private int alphaChannel1;
        private int offsetInTiles1;

        private readonly TextboxValidator textboxValidator;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public SaveTileMap window;
        public SaveTileMapViewModel(SaveTileMap window)
        {
            textboxValidator = new TextboxValidator();
            textboxValidator.ErrorsChanged += TextboxValidator_ErrorsChanged;

            SaveCommand     = ReactiveCommand.Create(Save);

            this.window = window;
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
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            return textboxValidator.GetErrors(propertyName);
        }

        public bool HasErrors => textboxValidator.HasErrors;
    }
}
