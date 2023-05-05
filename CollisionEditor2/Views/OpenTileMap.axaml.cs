using Avalonia.Controls;

namespace CollisionEditor2.Views
{
    public partial class OpenTileMap : Window
    {
        public bool IsOpened { get; set; } = false;
        public int TileHeight { get; set; } = 0;
        public int TileWidth { get; set; } = 0;
        public int VerticalSeparation { get; set; } = 0;
        public int HorizontalSeparation { get; set; } = 0;
        public int VerticalOffset { get; set; } = 0;
        public int HorizontalOffset { get; set; } = 0;
        public OpenTileMap()
        {
            InitializeComponent();
        }
    }
}
