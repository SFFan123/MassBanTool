using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MassBanToolMP.ViewModels;
using MassBanToolMP.Views;

namespace MassBanToolMP
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                
                desktop.MainWindow = new MainWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                desktop.MainWindow.DataContext = new MainWindowViewModel();

                ((desktop.MainWindow.DataContext as MainWindowViewModel)!).DataGrid =
                    desktop.MainWindow.Get<DataGrid>("DataGrid");
            }

            base.OnFrameworkInitializationCompleted();
        }

        public Window? MainWindow
        {
            get => (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }
    }
}
