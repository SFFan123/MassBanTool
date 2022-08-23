using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class EditIeNumerableDialog : Window
    {
        public EditIeNumerableDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
