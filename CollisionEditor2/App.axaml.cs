using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CollisionEditor2.ViewModels;
using CollisionEditor2.Views;

namespace CollisionEditor2
{   
   
    public partial class App : Application
    {
        public MainViewModel windowMain { get; set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow window = new();
                windowMain = new MainViewModel(window);
                window.windowMain = windowMain;
                desktop.MainWindow = window;
                desktop.MainWindow.DataContext = windowMain;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}