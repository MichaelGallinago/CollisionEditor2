using Avalonia.Controls;

namespace CollisionEditor2.Views
{
    public partial class SaveTileMap : Window
    {
        public bool IsSaved { get; set; } = false;
        public Avalonia.Media.IImage ResultSaveImage { get; set; }
        public SaveTileMap()
        {
            InitializeComponent();
        }
    }
}