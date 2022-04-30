using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Metadata;
using DynamicData;
using MassBanToolMP.Models;
using ReactiveUI;
using TwitchLib.Client.Events;

namespace MassBanToolMP.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string MESSAGE_DELAY_TOO_LOW = "Delay may not be under 300ms";
        private const string MESSAGE_DELAY_INVALID_TYPE = "Message Delay must be a postive number.";
        private const string CHANNEL_INVALID = "Invalid Channel Name";

        private readonly string allowedActions;
        private string _channel_s = String.Empty;
        private int _messageDelay = 301;
        private string _oAuth;
        private string _username;
        private List<string> channels = new List<string>();

        public DataGrid DataGrid;
        private TwitchChatClient? twitchChatClient;
        private Mutex userMutex;
        private int _banProgress;

        public MainWindowViewModel()
        {
            ExitCommand = ReactiveCommand.Create<Window>(Exit);
            DebugCommand = ReactiveCommand.Create(Debug);
            OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
            ConnectCommand = ReactiveCommand.Create(Connect);
            LoadCredentialsCommand = ReactiveCommand.Create(LoadCredentials);

            Entries = new ObservableCollection<Entry>();

            allowedActions = string.Join("\n", Helper.DefaultAllowedActions);
        }

        private async void LoadCredentials()
        {
            try
            {
                var cred = SecretHelper.GetCredentials();
                Username = cred.Item1;
                OAuth = cred.Item2;
            }
            catch (Exception e)
            {
                //TODO
                Console.WriteLine(e);
            }
        }

        public bool CanConnect
        {
            get
            {
                bool haserr = false;
                var errs = GetErrors(nameof(Channel_s));
                if (errs is List<string> l_errs)
                {
                    haserr = l_errs.Count > 0;
                }

                return !string.IsNullOrEmpty(Username)
                       && !string.IsNullOrEmpty(OAuth)
                       && channels.Count > 0
                       && !haserr;
            }
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string OAuth
        {
            get => _oAuth;
            set => SetProperty(ref _oAuth, value);
        }

        public string MessageDelay
        {
            get => _messageDelay.ToString();
            set
            {
                if (int.TryParse(value, out int val))
                {
                    ClearError(nameof(MessageDelay), MESSAGE_DELAY_INVALID_TYPE);
                }
                else
                {
                    AddError(nameof(MessageDelay), MESSAGE_DELAY_INVALID_TYPE);
                }

                if (val < 300)
                {
                    AddError(nameof(MessageDelay), MESSAGE_DELAY_TOO_LOW);
                }
                else
                {
                    ClearError(nameof(MessageDelay), MESSAGE_DELAY_TOO_LOW);
                }

                SetProperty(ref _messageDelay, val);
                // Send to Client.
            }
        }

        public string Channel_s
        {
            get => _channel_s;
            set
            {
                string[] cache = value.Split(",");

                if (cache.Any(x => x.Trim().Length < 3))
                {
                    AddError(nameof(Channel_s), CHANNEL_INVALID);
                }
                else
                {
                    ClearError(nameof(Channel_s));
                }

                SetProperty(ref _channel_s, value);
                channels = cache.ToList();
                RaisePropertyChanged(nameof(CanConnect));
            }
        }

        public int BanProgress
        {
            get => _banProgress;
            set
            {
                SetProperty(ref _banProgress, value);
                RaisePropertyChanged(nameof(ETA));
            } 
        }


        private ReactiveCommand<Window, Unit> ExitCommand { get; }
        private ReactiveCommand<Unit, Unit> DebugCommand { get; }
        private ReactiveCommand<Window, Unit> OpenFileCommand { get; }

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCredentialsCommand { get; }

        public ObservableCollection<Entry> Entries { get; set; }

        public string ReadFileAllowedActions
        {
            get => allowedActions;
        }

        public Dictionary<string, List<string>> ChannelModerators { get; private set; } =
            new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> ChannelVIPs { get; private set; } =
            new Dictionary<string, List<string>>();

        public List<string> allSpecialChannelUser
        {
            get
            {
                List<string> result = new List<string>();
                foreach (var elm in ChannelModerators)
                {
                    result.AddRange(elm.Value);
                }

                foreach (var elm in ChannelVIPs)
                {
                    result.AddRange(elm.Value);
                }

                return result;
            }
        }

        [DependsOn(nameof(MessageDelay))]
        [DependsOn(nameof(Entries))]
        [DependsOn(nameof(Channel_s))]
        public TimeSpan ETA
        {
            get
            {
                int milisecondseconds = Entries.Count * channels.Count * _messageDelay;
                return TimeSpan.FromMilliseconds(milisecondseconds);
            }
        }

        private void Exit(Window obj)
        {
            obj.Close();
        }

        async void Debug()
        {
            BanProgress += 5;
            //string header = "Test";
            //DataGrid.Columns.Add(new DataGridTextColumn() {Header = header, Binding = new Binding(){Path = $"Result[_{header}]"} });
        }


        void AddChannelsToGrid()
        {
            foreach (string channel in channels)
            {
                DataGrid.Columns.Add(new DataGridTextColumn() { Header = channel });
            }
        }

        void RemoveChannelsFromGrid(List<string> toRemove = null)
        {
            var _toRemove = DataGrid.Columns.Skip(2);
            if (toRemove != null)
            {
                _toRemove.Where(x => toRemove.Contains(x.Header));
            }

            DataGrid.Columns.RemoveMany(_toRemove);
        }


        private bool CheckMutex()
        {
            try
            {
                userMutex = new Mutex(true, "MassBanTool_" + Username);
                return userMutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void Connect()
        {
            if (!CanConnect)
            {
                return;
            }


            if (!CheckMutex())
            {
                await MessageBox.Show("Only one instance with the same username allowed to run.", "Mutex error");
                return;
            }

            twitchChatClient = new TwitchChatClient(this, _username, _oAuth, channels);

            twitchChatClient.client.OnIncorrectLogin += IncorrectLogin;
            twitchChatClient.client.OnUserBanned += OnUserBanned;
            twitchChatClient.client.OnVIPsReceived += Client_OnVIPsReceived;
            twitchChatClient.client.OnModeratorsReceived += Client_OnModeratorsReceived;
            twitchChatClient.client.OnMessageThrottled += Client_OnMessageThrottled;
        }

        private void Client_OnMessageThrottled(object? sender, TwitchLib.Communication.Events.OnMessageThrottledEventArgs e)
        {
            MessageDelay += 10;
        }

        private void Client_OnModeratorsReceived(object? sender, OnModeratorsReceivedArgs e)
        {
            ChannelModerators[e.Channel.ToLowerInvariant()] = e.Moderators;
        }

        private void Client_OnVIPsReceived(object? sender, OnVIPsReceivedArgs e)
        {
            ChannelVIPs[e.Channel.ToLowerInvariant()] = e.VIPs;
        }

        private async void IncorrectLogin(object? sender, OnIncorrectLoginArgs e)
        {
            await MessageBox.Show("Login incorrect.", "Login error");
        }

        public async void MissingPermissions(string userStateChannel)
        {
            string toRemove = channels.First(x => x.Equals(userStateChannel, StringComparison.CurrentCultureIgnoreCase));
            channels.Remove(toRemove);

            _channel_s = string.Join(", ", channels);
            RaisePropertyChanged(nameof(Channel_s));

            await MessageBox.Show("Channel: " + userStateChannel + Environment.NewLine + "Disconnecting.", "Missing Permissions in channel.");

            twitchChatClient = null;
        }

        private void OnUserBanned(object? sender, OnUserBannedArgs e)
        {
            var item = Entries.FirstOrDefault(x =>
                x.Name.Equals(e.UserBan.Username, StringComparison.InvariantCultureIgnoreCase));

            if (item != null)
            {
                item.Result[e.UserBan.Channel] = "BANNED";
            }
        }


        private async void OpenFile(Window window)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.AllowMultiple = false;
            diag.Title = "Open File:";
            string[]? path = await diag.ShowAsync(window);

            if (path == null || path.Length == 0)
            {
                return;
            }

            string[] lines;
            // Check file can be open
            try
            {
                // read all text
                lines = await File.ReadAllLinesAsync(path.First());
            }
            catch (Exception e)
            {
                await MessageBox.Show("Error while opening file:\n" + e.Message, "Error reading file.");
                return;
            }

            List<string> rows = new List<string>();
            // iterate over each line cleaning

            string raw = String.Empty;
            foreach (string line in lines)
            {
                raw = line.Trim();
                if (!string.IsNullOrEmpty(raw))
                {
                    rows.Add(raw);
                }
            }


            // TODO check listtype

            Regex userlistRegex = new Regex(@"^\w{2,}$", RegexOptions.Compiled);
            Regex readfileRegex = new Regex(@"^(\.|\/\w+) (\w{2,}) ?(.+)?$", RegexOptions.Compiled);

            List<Entry> entryList = new List<Entry>();

            // display.
            foreach (string row in rows)
            {
                bool isUser = userlistRegex.IsMatch(row);
                var readFileMatch = readfileRegex.Match(row);

                if (isUser)
                {
                    entryList.Add(
                        new Entry()
                        {
                            Name = row
                        }
                    );
                }
                else if (readFileMatch.Success)
                {
                    entryList.Add(
                        new Entry()
                        {
                            Command = readFileMatch.Groups[1].Value,
                            Name = readFileMatch.Groups[2].Value,
                            Reason = readFileMatch.Groups[3].Value
                        }
                    );
                }
                else
                {
                    //TODO
                    //log
                }
            }

            Entries.Clear();
            Entries.AddRange(entryList);
            RaisePropertyChanged(nameof(Entries));
        }
    }
}