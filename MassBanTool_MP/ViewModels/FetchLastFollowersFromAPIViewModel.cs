using System.Reactive;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace MassBanToolMP.ViewModels
{
    public class FetchLastFollowersFromAPIViewModel : ViewModelBase
    {
        public FetchLastFollowersFromAPIViewModel()
        {
            _fetchAmount = 200;
            _channel = string.Empty;
            CloseOKCommand = ReactiveCommand.Create<Window>(CloseWindowOK, 
                this.WhenAnyValue(x => x.Channel, channel => channel!=null && channel.Length>2));
            CloseCancelCommand = ReactiveCommand.Create<Window>(CloseWindowCancel);
        }
        private uint _fetchAmount;
        private string _channel;

        public uint FetchAmount
        {
            get => _fetchAmount;
            set => SetProperty(ref _fetchAmount, value);
        }

        public string Channel
        {
            get => _channel;
            set
            {
                if (value.Length < 2)
                {
                    AddError(nameof(Channel), "Invalid channel name");
                    return;
                }
                ClearError(nameof(Channel));
                SetProperty(ref _channel, value);
            }
        }
        

        public void CloseWindowOK(Window window)
        {
            if (HasErrors)
            {
                return;
            }
            window.Close(ButtonResult.Ok);
        }

        public void CloseWindowCancel(Window window)
        {
            window.Close(ButtonResult.Cancel);
        }

        public ReactiveCommand<Window, Unit> CloseOKCommand { get; }
        public ReactiveCommand<Window, Unit> CloseCancelCommand { get; }
    }
}
