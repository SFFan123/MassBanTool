using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class FetchLastFollowersFromAPIDialog : Window
    {
        public FetchLastFollowersFromAPIDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close(ButtonResult.Abort);
            }
        }
    }
}
