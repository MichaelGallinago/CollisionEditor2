using Avalonia.Controls;
using Avalonia.Interactivity;
using CollisionEditor2.ViewModels;

namespace CollisionEditor2.Views
{
    public partial class OpenTileMap : Window
    {
        public OpenTileMapViewModel OpenTileMapMainViewModel { get; }
        public OpenTileMap()
        {
            InitializeComponent();
            OpenTileMapMainViewModel = new OpenTileMapViewModel(this);
        }

        private void SaveButtonClick(object? sender, RoutedEventArgs e)
        {   
            
            Close();
        }
    }
}
