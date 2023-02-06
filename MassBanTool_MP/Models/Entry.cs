using IX.Observable;
using MassBanToolMP.ViewModels;

namespace MassBanToolMP.Models
{
    public class Entry:ViewModelBase
    {
        private string command;
        private string name;
        private string reason;
        private ConcurrentObservableDictionary<string, string> result = new ();
        private bool _isValid = true;
        private string rowBackColor = "Blue";
        private string id;


        public string Command
        {
            get => command;
            set => SetProperty(ref command, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Reason
        {
            get => reason;
            set => SetProperty(ref reason, value);
        }

        public string ChatCommand
        {
            get => $"{Command} {Name} {Reason}".Trim();
        }

        public ConcurrentObservableDictionary<string, string> Result
        {
            get => result;
            set => SetProperty(ref result, value);
        }

        /// <summary>
        /// If twitch returns a bad ban target for this name.
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }
        public string RowBackColor
        {
            get => rowBackColor;
            set => SetProperty(ref rowBackColor, value);
        }
    }
}
