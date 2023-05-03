using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CollisionEditor2.ViewModels;
using CollisionEditor2.Views;

namespace CollisionEditor2
{
    public partial class App : Application
    {
        public MainViewModel? WindowMain { get; set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow window = new();
                desktop.MainWindow = window;
                desktop.MainWindow.DataContext = window.WindowMain;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}