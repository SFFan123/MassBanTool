using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class TextInputDialog : Window
    {
        private string _boxContent;

        // Only here for the build.
        public TextInputDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;
        }

        public TextInputDialog(string windowTitle, string labelText, Regex? boxValidationRegex = null)
        {
            BoxContent = string.Empty;
            WindowTitle = windowTitle;
            LabelText = labelText;
            BoxValidationRegex = boxValidationRegex;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;
            
            Opened += delegate
            {
                this.FindControl<TextBox>("InputBox")?.Focus();
            };
            KeyUp += delegate(object? _, KeyEventArgs args)
            {
                if (args.Key == Key.Escape)
                    Close(ButtonResult.Abort);
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public string WindowTitle { get; }
        public string LabelText { get; }
        
        public string BoxContent
        {
            get => _boxContent;
            set
            {
                _boxContent = value;
                if (BoxValidationRegex != null)
                {
                    if (!BoxValidationRegex.IsMatch(value))
                    {
                        throw new DataValidationException("Validation failed");
                    }
                }
            }
        }

        public Regex? BoxValidationRegex { get; }
        
        private void OkButtonClick(object? sender, RoutedEventArgs e)
        {
            Close(ButtonResult.Ok);
        }

        private void CancelButtonClick(object? sender, RoutedEventArgs e)
        {
            Close(ButtonResult.Cancel);
        }

        private void InputBox_OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Close(ButtonResult.Ok);
        }
    }
}
