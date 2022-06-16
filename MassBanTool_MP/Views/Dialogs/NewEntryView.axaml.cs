using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class NewEntryView : Window
    {
        public NewEntryView()
        {
            InitializeComponent();
            DataContext = this;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string command { get; set; }

        public string Username { get; set; }

        public string reason { get; set; }
        public bool result { get; private set; } = false;

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Username.Trim()))
            {
                this.FindControl<TextBox>("txtUsername").BorderBrush = new SolidColorBrush(Colors.Yellow);
                return;
            }
            result = true;
            Close();
        }
    }
}
