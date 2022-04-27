using System.Threading.Tasks;
using Avalonia.Controls;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using avMBox = MessageBox.Avalonia;

namespace MassBanToolMP
{
    internal class MessageBox
    {
        public static async Task<ButtonResult> Show(string message, string title, ButtonEnum Buttons = ButtonEnum.Ok)
        {
            Window owner = (App.Current as App).MainWindow;

            var messageBoxCustomWindow = avMBox.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams() {
                ContentMessage = message,
                ContentTitle = title,
                ButtonDefinitions = Buttons,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            return await messageBoxCustomWindow.ShowDialog(owner);
        }
        
    }
}
