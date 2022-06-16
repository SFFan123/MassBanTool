using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Metadata;
using DynamicData;
using MassBanToolMP.Models;
using MassBanToolMP.Views.Dialogs;
using ReactiveUI;
using TwitchLib.Client.Events;

namespace MassBanToolMP.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string MESSAGE_DELAY_TOO_LOW = "Delay may not be under 300ms";
        private const string MESSAGE_DELAY_INVALID_TYPE = "Message Delay must be a postive number.";
        private const string CHANNEL_INVALID = "Invalid Channel Name";

        private string _allowedActions;
        private int _banProgress;
        private string _channel_s = String.Empty;
        private int _messageDelay = 301;
        private string _oAuth;
        private string _reason;
        private string _username;
        private List<string> channels = new List<string>();

        public DataGrid DataGrid;
        private TwitchChatClient? twitchChatClient;
        private Mutex userMutex;

        private Task Worker;
        private ListType listType;

        public MainWindowViewModel()
        {
            ExitCommand = ReactiveCommand.Create<Window>(Exit);
            DebugCommand = ReactiveCommand.Create(Debug);
            OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
            ConnectCommand = ReactiveCommand.Create(Connect);
            LoadCredentialsCommand = ReactiveCommand.Create(LoadCredentials);
            OnClickPropertiesAddEntry = ReactiveCommand.Create<Window>(HandleAddEntry);
            OnClickPropertiesPasteClipboard = ReactiveCommand.Create(HandlePasteEntries);
            OnDataGridRemoveEntry = ReactiveCommand.Create<object>(RemoveEntry);
            OnClickPauseActionCommand = ReactiveCommand.Create(PauseAction);
            OnClickCancelActionCommand = ReactiveCommand.Create(CancelAction);
            RunBanCommand = ReactiveCommand.Create(ExecBan);
            RunUnbanCommand = ReactiveCommand.Create(ExecUnban);
            RunReadfileCommand = ReactiveCommand.Create(ExecReadfile);
            RunListFilterCommand = ReactiveCommand.Create(ExecListFilter);
            RunRemoveClutterCommand = ReactiveCommand.Create(ExecRemoveClutter);
            RunCheckListTypeCommand = ReactiveCommand.Create(CheckListType);
            RunSortListCommand = ReactiveCommand.Create(SortList);
            RunRemoveNotAllowedActionsCommand = ReactiveCommand.Create(RemoveNotAllowedActions);

            Entries = new ObservableCollection<Entry>();

            _allowedActions = string.Join("\n", Defaults.AllowedActions);
            listType = default;
        }

        private void CheckListType()
        {
            CheckListType(true);
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

        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
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

        public bool IsConnected
        {
            get
            {
                if (twitchChatClient?.client != null)
                {
                    return twitchChatClient.client.IsConnected && twitchChatClient.client.JoinedChannels.Any();
                }

                return false;
            }
        }

        [DependsOn(nameof(IsConnected))]
        public string ConButtonText
        {
            get
            {
                if (IsConnected)
                {
                    return "Switch Channel(s)";
                }

                return "Connect";
            }
        }

        public ListType ListType
        {
            get => listType;
            set => SetProperty(ref listType, value);
        }


        private ReactiveCommand<Window, Unit> ExitCommand { get; }
        private ReactiveCommand<Unit, Unit> DebugCommand { get; }
        private ReactiveCommand<Window, Unit> OpenFileCommand { get; }

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCredentialsCommand { get; }
        public ReactiveCommand<Window, Unit> OnClickPropertiesAddEntry { get; }
        public ReactiveCommand<Unit, Unit> OnClickPropertiesPasteClipboard { get; }
        public ReactiveCommand<Unit, Unit> RunBanCommand { get; }
        public ReactiveCommand<Unit, Unit> RunUnbanCommand { get; }
        public ReactiveCommand<Unit, Unit> RunReadfileCommand { get; }
        public ReactiveCommand<Unit, Unit> RunListFilterCommand { get; }
        public ReactiveCommand<Unit, Unit> RunRemoveClutterCommand { get; }
        public ReactiveCommand<Unit, Unit> RunCheckListTypeCommand { get; }
        public ReactiveCommand<Unit, Unit> RunSortListCommand { get; }
        public ReactiveCommand<Unit, Unit> RunRemoveNotAllowedActionsCommand { get; }
        public ReactiveCommand<Unit, Unit> OnClickPauseActionCommand { get; }
        public ReactiveCommand<Unit, Unit> OnClickCancelActionCommand { get; }

        public ReactiveCommand<object, Unit> OnDataGridRemoveEntry { get; }

        public ObservableCollection<Entry> Entries { get; set; }

        public string ReadFileAllowedActions
        {
            get => _allowedActions;
            set => SetProperty(ref _allowedActions, value);
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

        private void RemoveNotAllowedActions()
        {
            // get listtype

            // get list commands

            // filter

            throw new NotImplementedException();
        }

        private void SortList()
        {
            throw new NotImplementedException();
        }

        private void CheckListType(bool setBusy)
        {
            if (setBusy)
                IsBusy = true;

            if (Entries.Count == 0)
            {
                //($"INFO: List is empty.");
                ListType = ListType.None;
                return;
            }

            ListType = ListType.None;
            int listEnumerator = 0;
            foreach (Entry entry in Entries)
            {
                if (!string.IsNullOrEmpty(entry.Command) && (entry.Command.StartsWith("/") || entry.Command.StartsWith(".")))
                {
                    if (ListType == ListType.None || ListType == ListType.ReadFile)
                    {
                        ListType = ListType.ReadFile;
                    }
                    else
                    {
                        //log($"INFO: Line {listEnumerator} -> '{entry.Text}' --- triggered Listtype Mixed");
                        ListType = ListType.Mixed;
                        entry.BackColor = Program.Yellow;
                    }

                    if (ListType == ListType.Mixed)
                    {
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(entry.Name) && entry.Name.Contains(" "))
                {
                    //log($"INFO: Line {listEnumerator} -> '{entry.Text}' --- triggered Listtype Malformed");
                    ListType = ListType.Malformed;
                    entry.BackColor = Program.Red;
                    return;
                }

                listEnumerator++;
            }

            ListType = ListType == ListType.ReadFile ? ListType.ReadFile : ListType.UserList;

            if (setBusy)
                IsBusy = false;
        }

        private void ExecRemoveClutter()
        {
            throw new NotImplementedException();
        }

        private void ExecListFilter()
        {
            throw new NotImplementedException();
        }

        private void ExecReadfile()
        {
            throw new NotImplementedException();
        }

        private void ExecUnban()
        {
            throw new NotImplementedException();
        }

        private void ExecBan()
        {
            Worker = CreateWorkerTask(WorkingMode.Ban);
        }

        private void CancelAction()
        {
            throw new NotImplementedException();
        }

        private void PauseAction()
        {
            throw new NotImplementedException();
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


        private void AddChannelToGrid(string eChannel)
        {
            if (DataGrid.Columns.Any(x => (x.Header as string) == eChannel))
            {
                return;
            }

            DataGrid.Columns.Add(new DataGridTextColumn()
                { Header = eChannel, Binding = new Binding() { Path = $"Result[_{eChannel}]" } });
        }

        private void RemoveChannelToGrid(string eChannel)
        {
            var toremove = DataGrid.Columns.Skip(2).FirstOrDefault(x => (x.Header as string) == eChannel);
            DataGrid.Columns.Remove(toremove);
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

            if (IsConnected)
            {
                SwitchChannel();
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
            twitchChatClient.client.OnConnected += ClientOnOnConnected;


            foreach (string channel in channels)
            {
                AddChannelToGrid(channel);
            }
        }

        private void ClientOnOnConnected(object? sender, OnConnectedArgs e)
        {
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(ConButtonText));
        }

        private void SwitchChannel()
        {
            if (twitchChatClient == null)
            {
                // Should not be possible.
                return;
            }

            var joinedChannels = twitchChatClient.client.JoinedChannels.Select(x => x.Channel.ToLower());

            var toleave = joinedChannels.Except(channels).ToList();

            var tojoin = channels.Except(joinedChannels).ToList();

            foreach (var channel in toleave)
            {
                twitchChatClient.client.LeaveChannel(channel);
                ChannelModerators[channel]?.Clear();
                RemoveChannelToGrid(channel);
            }

            foreach (var channel in tojoin)
            {
                twitchChatClient.client.JoinChannel(channel, true);
                AddChannelToGrid(channel);
            }
        }

        private void Client_OnMessageThrottled(object? sender,
            TwitchLib.Communication.Events.OnMessageThrottledEventArgs e)
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
            string toRemove =
                channels.First(x => x.Equals(userStateChannel, StringComparison.CurrentCultureIgnoreCase));
            channels.Remove(toRemove);

            _channel_s = string.Join(", ", channels);
            RaisePropertyChanged(nameof(Channel_s));

            await MessageBox.Show("Channel: " + userStateChannel + Environment.NewLine + "Disconnecting.",
                "Missing Permissions in channel.");

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

        private async void HandleAddEntry(Window window)
        {
            var diag = new NewEntryView();
            await diag.ShowDialog(window);

            if (!diag.result)
                return;

            string name = diag.name.Trim();

            if (string.IsNullOrEmpty(diag.name))
            {
                // TODO warning?
                return;
            }

            Entries.Add(
                new Entry()
                {
                    Command = diag.command,
                    Name = name,
                    Reason = diag.reason
                });
        }

        private void RemoveEntry(object selectedItems)
        {
            if (selectedItems is IList item)
            {
                List<Entry> items = new List<Entry>();
                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] is Entry entry)
                        items.Add(entry);
                }

                Entries.RemoveMany(items);
            }
        }

        private async void HandlePasteEntries()
        {
            var text = await Application.Current.Clipboard.GetTextAsync();

            var lines = text.Split(Environment.NewLine);

            SetLines(lines);
            CheckListType(true);
        }

        private async void OpenFile(Window window)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.AllowMultiple = false;
            diag.Title = "Open File:";
            string[]? path = await diag.ShowAsync(window);

            IsBusy = true;

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

            SetLines(lines);
            CheckListType(false);
            IsBusy = false;
        }

        private void SetLines(IEnumerable<string> lines)
        {
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


        private bool FilterEntriesForSpecialUsers()
        {
            List<Entry> toRemove = new List<Entry>();
            foreach (Entry entry in Entries)
            {
                if (allSpecialChannelUser.Any(x =>
                        string.Equals(x, entry.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    toRemove.Add(entry);
                }
            }

            bool res = toRemove.Any();

            // TODO get if stop here.
            //if()

            foreach (Entry entry in toRemove)
            {
                Entries.Remove(entry);
            }

            RaisePropertyChanged(nameof(Entries));
            return res;
        }

        private async void CheckReadlistForIllegalCommands()
        {
            string[] allowedActions = _allowedActions.Split(Environment.NewLine);

            if (allowedActions.Length == 0)
            {
                await MessageBox.Show("No allowed Action", "Readfile Warning");
                throw new InvalidOperationException();
            }

            var query = Entries.AsParallel().Where(x =>
                !allowedActions.Contains(x.Command, StringComparer.Create(CultureInfo.CurrentCulture, true)));

            if (query.Any())
            {
                await MessageBox.Show("Mismatch between allowed commands and commands used in the file.", "Warning");
                throw new InvalidOperationException();
            }


            throw new NotImplementedException();
        }

        private void CheckForProtectedUser()
        {
            //TODO
        }


        private Task CreateWorkerTask(WorkingMode mode)
        {
            if (twitchChatClient == null)
            {
                throw new ArgumentException();
            }

            return Task.Factory.StartNew(async () =>
            {
                string TextReason = Reason;
                for (int i = 0; i < Entries.Count; i++)
                {
                    Entry entry = Entries[i];
                    string commandtoExecute = String.Empty;

                    switch (mode)
                    {
                        case WorkingMode.Ban:
                        {
                            // TODO protect VIP/MODS
                            commandtoExecute = $"/ban {entry.Name} {Reason}";
                            break;
                        }
                        case WorkingMode.Unban:
                        {
                            commandtoExecute = $"/unban {entry.Name}";
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }

                    foreach (var channel in twitchChatClient.client.JoinedChannels)
                    {
                        twitchChatClient.client.SendMessage(channel, commandtoExecute);
                        await Task.Delay(TimeSpan.FromMilliseconds(_messageDelay));
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }

    internal enum WorkingMode
    {
        Ban,
        Unban,
        Readfile
    }

    public enum ListType
    {
        None = default,
        UserList,
        ReadFile,
        Mixed,
        Malformed,
    }
}