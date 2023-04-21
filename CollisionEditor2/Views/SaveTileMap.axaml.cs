using Avalonia.Controls;

namespace CollisionEditor2.Views
{
    public partial class SaveTileMap : Window
    {
        public bool IsSaved = false;
        public int TileHeight = 0;
        public int TileWidth = 0;
        public int VerticalSeparation = 0;
        public int HorizontalSeparation = 0;
        public int VerticalOffset = 0;
        public int HorizontalOffset = 0;
        public SaveTileMap()
        {
            InitializeComponent();
        }
    }
}