using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using avMBox = MessageBox.Avalonia;

namespace MassBanToolMP
{
    internal class MessageBox
    {
        public static async Task<ButtonResult> Show(string message, string title, ButtonEnum Buttons = ButtonEnum.Ok)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return await InternalShow(message, title, Buttons);
            }
            else
            {
                return await Dispatcher.UIThread.InvokeAsync(() => InternalShow(message, title, Buttons));
            }
        }

        private static async Task<ButtonResult> InternalShow(string message, string title, ButtonEnum Buttons = ButtonEnum.Ok)
        {
            Window? owner = (App.Current as App)?.MainWindow;

            if (owner != null)
            {
                var messageBoxCustomWindow = avMBox.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams() {
                    ContentMessage = message,
                    ContentTitle = title,
                    ButtonDefinitions = Buttons,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = true
                });
                return await messageBoxCustomWindow.ShowDialog(owner);
            }
            else
            {
                var messageBoxCustomWindow = avMBox.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams() {
                    ContentMessage = message,
                    ContentTitle = title,
                    ButtonDefinitions = Buttons,
                    CanResize = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });
                return await messageBoxCustomWindow.Show();
            }
        }
        
    }
}
