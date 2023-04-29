using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace CollisionEditor2.Views
{
    public partial class SaveTileMap : Window
    {
        public bool IsSaved { get; set; } = false;
        public Bitmap ResultSaveImage { get; set; }
        public SaveTileMap()
        {
            InitializeComponent();
        }
    }
}