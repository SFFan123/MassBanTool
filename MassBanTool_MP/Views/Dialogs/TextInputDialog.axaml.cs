using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia.Enums;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class TextInputDialog : Window
    {
        private string _boxContent;

        // Only here for the build.
        public TextInputDialog()
        {
            throw new InvalidOperationException();
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
    }
}
