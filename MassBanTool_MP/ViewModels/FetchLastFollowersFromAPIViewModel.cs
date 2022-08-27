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
            CloseOKCommand = ReactiveCommand.Create<Window>(CloseWindowOK, 
                this.WhenAnyValue(x => x.HasErrors, errs => !errs));
            CloseCancelCommand = ReactiveCommand.Create<Window>(CloseWindowCancel);
            AddError("init", "Channel is Required");
            Result = ButtonResult.None;
            _fetchAmount = 200;
            _channel = string.Empty;
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
                ClearError("init");
                SetProperty(ref _channel, value);
            }
        }

        public ButtonResult Result { get; private set; }


        public void CloseWindowOK(Window window)
        {
            if (HasErrors)
            {
                return;
            }
            Result = ButtonResult.Ok;
            window.Close();
        }

        public void CloseWindowCancel(Window window)
        {
            Result = ButtonResult.Cancel;
            window.Close();
        }

        public ReactiveCommand<Window, Unit> CloseOKCommand { get; }
        public ReactiveCommand<Window, Unit> CloseCancelCommand { get; }
    }
}
