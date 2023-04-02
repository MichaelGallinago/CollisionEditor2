using Avalonia.Controls;

namespace CollisionEditor2.Views
{
    public partial class OpenTileMap : Window
    {
        public OpenTileMap()
        {
            InitializeComponent();
            
        }

        public void SetOwner(Window window)
        {
            this.Owner = window;
        }
    }
}
