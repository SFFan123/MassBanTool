using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace MassBanToolMP
{
    public class MessageBox
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

        private static async Task<ButtonResult> InternalShow(string message, string title,
        ButtonEnum Buttons = ButtonEnum.Ok)
        {
            
            Window? owner = (Avalonia.Application.Current as App)?.MainWindow;

            if (owner != null && owner.IsVisible)
            {
                var messageBoxCustomWindow = MessageBoxManager.GetMessageBoxStandard( 
                    new MessageBoxStandardParams()
                    {
                        ContentMessage = message,
                        ContentTitle = title,
                        ButtonDefinitions =  Buttons,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = true
                    });
                return await messageBoxCustomWindow.ShowWindowDialogAsync(owner);
            }
            else
            {
                var messageBoxCustomWindow = MessageBoxManager.GetMessageBoxStandard(
                    new MessageBoxStandardParams()
                    {
                        ContentMessage = message,
                        ContentTitle = title,
                        ButtonDefinitions = Buttons,
                        CanResize = true,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    });
                return await messageBoxCustomWindow.ShowAsync();
            }
        }
    }
}