using System.Collections.Generic;
using Avalonia.Media;
using MassBanToolMP.ViewModels;

namespace MassBanToolMP.Models
{
    public class Entry:ViewModelBase
    {
        private string command;
        private string name;
        private string reason;
        private Dictionary<string, string> result = new Dictionary<string, string>();
        private string rowBackColor = "Blue";

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

        public string Reason
        {
            get => reason;
            set => SetProperty(ref reason, value);
        }

        public string ChatCommand
        {
            get => $"{Command} {Name} {Reason}".Trim();
        }


        public Dictionary<string, string> Result
        {
            get => result;
            set => SetProperty(ref result, value);
        }

        public string RowBackColor
        {
            get => rowBackColor;
            set => SetProperty(ref rowBackColor, value);
        }
    }
}
