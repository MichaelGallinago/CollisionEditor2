using Avalonia.Controls;
using SkiaSharp;

namespace CollisionEditor2.Views
{
    public partial class SaveTileMap : Window
    {
        public bool IsSaved { get; set; } = false;
        public SKBitmap ResultSaveImage { get; set; }
        public SaveTileMap()
        {
            InitializeComponent();
        }
    }
}