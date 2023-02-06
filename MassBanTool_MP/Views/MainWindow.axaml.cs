using Avalonia.Controls;
using Avalonia.Interactivity;
using MassBanToolMP.ViewModels;

namespace MassBanToolMP.Views
{
    public partial class MainWindow : BaseWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { ContextMenu: { } } btn)
            {
                btn.ContextMenu.Open(btn);
            }
        }
    }
}
