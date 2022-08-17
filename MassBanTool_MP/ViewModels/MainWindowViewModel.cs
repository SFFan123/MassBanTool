using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using DynamicData;
using MassBanToolMP.Helper;
using MassBanToolMP.Models;
using MassBanToolMP.Views;
using MassBanToolMP.Views.Dialogs;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using TwitchLib.Client.Events;

namespace MassBanToolMP.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string MESSAGE_DELAY_TOO_LOW = "Delay may not be under 300ms";
        private const string MESSAGE_DELAY_INVALID_TYPE = "Message Delay must be a postive number.";
        private const string CHANNEL_INVALID = "Invalid Channel Name";

        private const string HELP_URL_COOLDOWN =
            @"https://github.com/SFFan123/MassBanTool/wiki/Cooldown-between-Messages";

        private const string HELP_URL_REGEX_MS_DOCS =
            @"https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference";

        private const string HELP_URL_WIKI = @"https://github.com/SFFan123/MassBanTool/wiki";
        private const string HELP_URL_MAIN_GITHUB = "https://github.com/SFFan123/MassBanTool";
        private const string HELP_URL_REGEX101 = "https://regex101.com/";

        private static IDisposable canExecPauseAbortObservable;
        private static IDisposable isConnectedObservable;

        private string _allowedActions;
        private double _banProgress;
        private string _channelS = string.Empty;
        private ObservableCollection<Entry> _entries;
        private TimeSpan _eta;
        private int _messageDelay = 301;
        private string _oAuth;
        private bool _paused;
        private string _reason;
        private string _username;
        private Task _worker;
        private List<string> channels = new List<string>();

        public DataGrid DataGrid;
        private string filterRegex = string.Empty;

        private ContextMenu? _lastVisitedChannelsMenu;
        private bool _listFilterRemoveMatching = false;
        private ListType _listType;
        private bool _protectSpecialUsers = true;
        private CancellationToken _token;

        private LogWindow _logWindow;
        private readonly LogViewModel _logModel;

        private CancellationTokenSource _tokenSource;
        private TwitchChatClient? _twitchChatClient;
        private Mutex _userMutex;


        public MainWindowViewModel()
        {
            _logModel = new LogViewModel();
            LogViewModel.Log("Init GUI...");

            Entries = new ObservableCollection<Entry>();

            ExitCommand = ReactiveCommand.Create<Window>(Exit);
            DebugCommand = ReactiveCommand.Create(Debug);
            OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
            OpenFileFromURLCommand = ReactiveCommand.Create<Window>(OpenFileFromURL);

            ConnectCommand = ReactiveCommand.Create(Connect);

            LoadCredentialsCommand = ReactiveCommand.Create(LoadCredentials);
            OnClickPropertiesAddEntry = ReactiveCommand.Create<Window>(HandleAddEntry);
            OnClickPropertiesPasteClipboard = ReactiveCommand.Create(HandlePasteEntries);
            OnDataGridRemoveEntry = ReactiveCommand.Create<object>(RemoveEntry);

            OnClickPauseActionCommand = ReactiveCommand.Create(PauseAction);
            OnClickCancelActionCommand = ReactiveCommand.Create(CancelAction);

            RunBanCommand = ReactiveCommand.Create(ExecBan);
            RunUnbanCommand = ReactiveCommand.Create(ExecUnban);
            RunReadfileCommand = ReactiveCommand.Create(ExecReadFile);
            RunListFilterCommand = ReactiveCommand.Create(ExecListFilter);
            RunCheckListTypeCommand = ReactiveCommand.Create(CheckListType);
            RunSortListCommand = ReactiveCommand.Create(SortList);
            RunRemoveNotAllowedActionsCommand = ReactiveCommand.Create(RemoveNotAllowedActions);
            OpenWikiCommand = ReactiveCommand.Create(() => OpenURL(HELP_URL_WIKI));
            CooldownInfoCommand = ReactiveCommand.Create(() => OpenURL(HELP_URL_COOLDOWN));
            OpenRegexDocsCommand = ReactiveCommand.Create(() => OpenURL(HELP_URL_REGEX_MS_DOCS));
            OpenRegex101Command = ReactiveCommand.Create(() => OpenURL(HELP_URL_REGEX101));
            OpenGitHubPageCommand = ReactiveCommand.Create(() => OpenURL(HELP_URL_MAIN_GITHUB));
            ShowLogWindowCommand = ReactiveCommand.Create<Window>(ShowLogWindow);

            LoadData();
            _listType = default;
            LogViewModel.Log("Done Init GUI...");
        }

        

        private void CreateCommands()
        {

        }

        private Task Worker
        {
            get => _worker;
            set
            {
                if (SetProperty(ref _worker, value))
                {
                    canExecPauseAbortObservable?.Dispose();
                    canExecPauseAbortObservable = Worker.WhenAnyValue(x => x.IsCompleted)
                        .Subscribe(_ => RaisePropertyChanged(nameof(CanExecPauseAbort)));
                    RaisePropertyChanged(nameof(CanExecPauseAbort));
                    RaisePropertyChanged(nameof(CanExecRun));
                }
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

        public bool CanExecPauseAbort => Worker != null && !Worker.IsCompleted;

        public bool CanExecRun => IsConnected && Entries.Count > 0 && (Worker == null || Worker.IsCompleted);


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
            get => _channelS;
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

                SetProperty(ref _channelS, value);
                channels = cache.ToList();
                RaisePropertyChanged(nameof(CanConnect));
            }
        }

        public double BanProgress
        {
            get => _banProgress;
            set => SetProperty(ref _banProgress, value);
        }

        public bool IsConnected
        {
            get
            {
                if (_twitchChatClient?.client != null)
                {
                    return _twitchChatClient.client.IsConnected && _twitchChatClient.client.JoinedChannels.Any();
                }

                return false;
            }
        }

        public bool Paused
        {
            get => _paused;
            set
            {
                if (SetProperty(ref _paused, value))
                    RaisePropertyChanged(nameof(PauseButtonText));
            }
        }

        public string PauseButtonText => Paused ? "Resume" : "Pause";


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
            get => _listType;
            set => SetProperty(ref _listType, value);
        }

        private ReactiveCommand<Window, Unit> ExitCommand { get; }
        private ReactiveCommand<Unit, Unit> DebugCommand { get; }
        private ReactiveCommand<Window, Unit> OpenFileCommand { get; }
        private ReactiveCommand<Window, Unit> OpenFileFromURLCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCredentialsCommand { get; }
        public ReactiveCommand<Window, Unit> OnClickPropertiesAddEntry { get; }
        public ReactiveCommand<Unit, Unit> OnClickPropertiesPasteClipboard { get; }
        public ReactiveCommand<Unit, Unit> RunBanCommand { get; }
        public ReactiveCommand<Unit, Unit> RunUnbanCommand { get; }
        public ReactiveCommand<Unit, Unit> RunReadfileCommand { get; }
        public ReactiveCommand<Unit, Unit> RunListFilterCommand { get; }
        public ReactiveCommand<Unit, Unit> RunCheckListTypeCommand { get; }
        public ReactiveCommand<Unit, Unit> RunSortListCommand { get; }
        public ReactiveCommand<Unit, Unit> RunRemoveNotAllowedActionsCommand { get; }
        public ReactiveCommand<Unit, Unit> OnClickPauseActionCommand { get; }
        public ReactiveCommand<Unit, Unit> OnClickCancelActionCommand { get; }
        public ReactiveCommand<object, Unit> OnDataGridRemoveEntry { get; }
        public ReactiveCommand<Unit, Unit> CooldownInfoCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenRegexDocsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenWikiCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGitHubPageCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenRegex101Command { get; }
        public ReactiveCommand<Window, Unit> ShowLogWindowCommand { get; }

        public ObservableCollection<Entry> Entries
        {
            get => _entries;
            set
            {
                if (SetProperty(ref _entries, value))
                {
                    RaisePropertyChanged(nameof(CanExecRun));
                }
            }
        }

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

        public TimeSpan ETA
        {
            get => _eta;
            set => SetProperty(ref _eta, value);
        }

        public bool ProtectSpecialUsers
        {
            get => _protectSpecialUsers;
            set => SetProperty(ref _protectSpecialUsers, value);
        }

        public bool ProtectedUserMode_Skip { get; set; }

        public bool ProtectedUserMode_Cancel { get; set; } = true;

        public string FilterRegex
        {
            get => filterRegex;
            set => SetProperty(ref filterRegex, value);
        }

        public bool RegexOptionIgnoreCase { get; set; }
        public bool RegexOptionMultiline { get; set; }

        public bool RegexOptionEcmaScript { get; set; }
        public bool RegexOptionCultureInvariant { get; set; }

        public bool ListFilterRemoveMatching
        {
            get => _listFilterRemoveMatching;
            set => SetProperty(ref _listFilterRemoveMatching, value);
        }

        public bool ListFilterRemoveNotMatching
        {
            get => !_listFilterRemoveMatching;
            set => SetProperty(ref _listFilterRemoveMatching, !value);
        }

        public ContextMenu? LastVisitedChannelsMenu
        {
            get => _lastVisitedChannelsMenu;
        }

        private void LoadData()
        {
            LogViewModel.Log("Try loading setting for this User.");
            string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");
            try
            {
                string a = File.ReadAllText(fileName);
                a = a.Trim();
                var data = DataWrapper.fromJson(a);

                if (data.message_delay != default)
                {
                    MessageDelay = data.message_delay.ToString();
                }

                if (data.lastVisitedChannel != null)
                {
                    _lastVisitedChannelsMenu = new ContextMenu();

                    MenuItem item;
                    List<MenuItem> items = new List<MenuItem>();
                    foreach (string s in data.lastVisitedChannel)
                    {
                        string header = s.Replace("_", "__");
                        item = new MenuItem()
                        {
                            Header = header,
                            DataContext = s
                        };
                        item.Click += delegate(object? sender, RoutedEventArgs args)
                        {
                            if (sender is MenuItem menuitem)
                            {
                                Channel_s = (string)menuitem.DataContext;
                            }
                        };
                        items.Add(item);
                    }

                    _lastVisitedChannelsMenu.Items = items;
                    RaisePropertyChanged(nameof(LastVisitedChannelsMenu));
                }
                else
                {
                    _lastVisitedChannelsMenu = null;
                }

                if (data.AllowedActions != null)
                {
                    ReadFileAllowedActions = string.Join(Environment.NewLine, data.AllowedActions);
                }
                else
                {
                    ReadFileAllowedActions = string.Join(Environment.NewLine, Defaults.AllowedActions);
                }
            }
            catch (Exception e)
            {
                LogViewModel.Log("Something went wrong loading setting for this User. - " + e.Message);
            }
            LogViewModel.Log("Done loading setting for this User.");
        }

        private void RaiseIsConnectedChanged()
        {
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(ConButtonText));
            RaisePropertyChanged(nameof(CanExecRun));
        }


        private void OpenURL(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void CheckListType()
        {
            CheckListType(true);
        }

        private void RemoveNotAllowedActions()
        {
            if (ListType is not (ListType.ReadFile or ListType.Mixed))
            {
                if (string.IsNullOrEmpty(ReadFileAllowedActions))
                {
                    //ERROR
                    return;
                }

                string[] actions = ReadFileAllowedActions.Split(Environment.NewLine);

                List<Entry> toRemove = Entries.AsParallel().Where(x => !actions.Contains(x.Command)).ToList();

                Entries.RemoveMany(toRemove);
            }
        }

        private void SortList()
        {
            Entries = new ObservableCollection<Entry>(Entries.OrderBy(i => i.Name));
        }

        private void CheckListType(bool setBusy)
        {
            if (setBusy)
                IsBusy = true;

            if (Entries.Count == 0)
            {
                LogViewModel.Log("INFO: List is empty.");
                ListType = ListType.None;
                if (setBusy)
                    IsBusy = false;
                return;
            }

            ListType = ListType.None;
            int listEnumerator = 0;
            foreach (Entry entry in Entries)
            {
                if (!string.IsNullOrEmpty(entry.Command) &&
                    (entry.Command.StartsWith("/") || entry.Command.StartsWith(".")))
                {
                    if (ListType == ListType.None || ListType == ListType.ReadFile)
                    {
                        ListType = ListType.ReadFile;
                    }
                    else
                    {
                        LogViewModel.Log($"INFO: Line {listEnumerator} -> '{entry.ChatCommand}' --- triggered Listtype Mixed");
                        ListType = ListType.Mixed;
                        entry.RowBackColor = "Yellow";
                    }

                    if (ListType == ListType.Mixed)
                    {
                        if (setBusy)
                            IsBusy = false;
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(entry.Name) && entry.Name.Contains(" "))
                {
                    LogViewModel.Log($"INFO: Line {listEnumerator} -> '{entry.ChatCommand}' --- triggered Listtype Malformed");
                    ListType = ListType.Malformed;
                    entry.RowBackColor = "Red";
                    if (setBusy)
                        IsBusy = false;
                    return;
                }

                listEnumerator++;
            }

            ListType = ListType == ListType.ReadFile ? ListType.ReadFile : ListType.UserList;

            if (setBusy)
                IsBusy = false;
        }
        
        private async void ExecListFilter()
        {
            Regex regex;

            try
            {
                regex = CreateFilterRegex();
            }
            catch (ArgumentException e)
            {
                await MessageBox.Show("Regex malforemed" + Environment.NewLine + e.Message,
                    "Exception while creating regex");
                return;
            }

            List<Entry> toRemove = Entries.AsParallel().Where(x =>
            {
                if (regex.IsMatch(x.ChatCommand))
                {
                    return !_listFilterRemoveMatching;
                }

                return _listFilterRemoveMatching;
            }).ToList();

            Entries.RemoveMany(toRemove);
        }

        private async void ExecReadFile()
        {
            if (!CanExecRun)
            {
                return;
            }

            if (ListType != ListType.ReadFile)
            {
                await MessageBox.Show("Listtype does not match the selected operation mode.", "Listtype Mismatch");
                return;
            }

            CheckForProtectedUser();
            CheckReadlistForIllegalCommands();

            Worker = CreateWorkerTask(WorkingMode.Readfile);
        }

        private async void ExecUnban()
        {
            if (!CanExecRun)
            {
                return;
            }

            if (ListType != ListType.ReadFile)
            {
                if (await MessageBox.Show(
                        "Listtype does not match the selected operation mode. Do you want to ignore the additional params and run this anyways?",
                        "Listtype Mismatch",
                        ButtonEnum.YesNo) != ButtonResult.Yes)
                {
                    return;
                }
            }

            Worker = CreateWorkerTask(WorkingMode.Unban);
        }

        private async void ExecBan()
        {
            if (!CanExecRun)
            {
                return;
            }

            if (ListType != ListType.ReadFile)
            {
                if (await MessageBox.Show(
                        "Listtype does not match the selected operation mode. Do you want to ignore the additional params and run this anyways?",
                        "Listtype Mismatch",
                        ButtonEnum.YesNo) != ButtonResult.Yes)
                {
                    return;
                }
            }

            CheckForProtectedUser();
            Worker = CreateWorkerTask(WorkingMode.Ban);
        }

        private void CancelAction()
        {
            _tokenSource.Cancel();
            try
            {
                Worker.Wait();
            }
            catch (AggregateException)
            {
            }

            ETA = TimeSpan.Zero;

            Worker.Dispose();
            canExecPauseAbortObservable.Dispose();
        }

        private void PauseAction()
        {
            Paused = !Paused;
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
                await MessageBox.Show("Failed to load credentials", "Credentials where?");
                Console.WriteLine(e);
            }
        }

        private void ShowLogWindow(Window owner)
        {
            if (_logWindow != null)
            {
                try
                {
                    if (!_logWindow.IsVisible)
                    {
                        _logWindow.Show(owner);
                        return;
                    }
                    _logWindow.Show(owner);
                    return;
                }
                catch (ObjectDisposedException)
                { }
            }

            _logWindow = new LogWindow()
            {
                DataContext = _logModel
            };
            _logWindow.Closed += (sender, args) =>
            {
                _logWindow = null;
                GC.Collect();
            };
            _logWindow.Show(owner);
        }

        private void Exit(Window obj)
        {
            obj.Close();
        }

        async void Debug()
        {
            LogViewModel.Log("Test");
        }

        private Regex CreateFilterRegex()
        {
            RegexOptions regexOptions = RegexOptions.Compiled;

            if (RegexOptionIgnoreCase)
                regexOptions |= RegexOptions.IgnoreCase;

            if (RegexOptionMultiline)
                regexOptions |= RegexOptions.Multiline;

            if (RegexOptionEcmaScript)
                regexOptions |= RegexOptions.ECMAScript;

            if (RegexOptionCultureInvariant)
                regexOptions |= RegexOptions.CultureInvariant;

            return new Regex(filterRegex, regexOptions);
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
                _userMutex = new Mutex(true, "MassBanTool_" + Username);
                return _userMutex.WaitOne(TimeSpan.FromMilliseconds(10), true);
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

            _twitchChatClient = new TwitchChatClient(this, _username, _oAuth, channels);

            _twitchChatClient.client.OnIncorrectLogin += IncorrectLogin;
            _twitchChatClient.client.OnUserBanned += OnUserBanned;
            _twitchChatClient.client.OnVIPsReceived += Client_OnVIPsReceived;
            _twitchChatClient.client.OnModeratorsReceived += Client_OnModeratorsReceived;
            _twitchChatClient.client.OnMessageThrottled += Client_OnMessageThrottled;
            _twitchChatClient.client.OnConnected += ClientOnOnConnected;

            if (!_twitchChatClient.client.IsConnected)
            {
                await MessageBox.Show("Failed to connect to twitch", "Warning");
            }

            foreach (string channel in channels)
            {
                AddChannelToGrid(channel);
            }

            isConnectedObservable = _twitchChatClient.WhenAnyValue(x => x.client.IsConnected)
                .Subscribe(_ => RaisePropertyChanged(nameof(IsConnected)));
        }

        private void ClientOnOnConnected(object? sender, OnConnectedArgs e)
        {
            RaiseIsConnectedChanged();
        }

        private void SwitchChannel()
        {
            if (_twitchChatClient == null)
            {
                LogViewModel.Log("Application reached an impossible state. No Twitch Client and trying to switch channels.");
                return;
            }

            var joinedChannels = _twitchChatClient.client.JoinedChannels.Select(x => x.Channel.ToLower());

            var toleave = joinedChannels.Except(channels).ToList();
            LogViewModel.Log($"Need to leave '{string.Join(", ", toleave)}'");

            var tojoin = channels.Except(joinedChannels).ToList();
            LogViewModel.Log($"Need to join '{string.Join(", ", tojoin)}'");

            LogViewModel.Log("Leaving Channels...");
            foreach (var channel in toleave)
            {
                _twitchChatClient.client.LeaveChannel(channel);
                ChannelModerators[channel]?.Clear();
                RemoveChannelToGrid(channel);
            }
            LogViewModel.Log("Joining Channels...");
            foreach (var channel in tojoin)
            {
                _twitchChatClient.client.JoinChannel(channel, true);
                AddChannelToGrid(channel);
            }
        }

        private void Client_OnMessageThrottled(object? sender, TwitchLib.Communication.Events.OnMessageThrottledEventArgs e)
        {
            LogViewModel.Log("Message throttle reached increasing Delay.");
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

            _channelS = string.Join(", ", channels);
            RaisePropertyChanged(nameof(Channel_s));

            await MessageBox.Show("Channel: " + userStateChannel + Environment.NewLine + "Disconnecting.",
                "Missing Permissions in channel.");

            _twitchChatClient = null;
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

            string name = diag.Username.Trim();

            if (string.IsNullOrEmpty(diag.Username))
            {
                await MessageBox.Show("Name/Target may not be empty", "Warning");
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
                LogViewModel.Log($"Reading File: {path.First()}...");
                // read all text
                lines = await File.ReadAllLinesAsync(path.First());
            }
            catch (Exception e)
            {
                string msg = "Error while opening file:\n" + e.Message;
                LogViewModel.Log("Error reading file: " + msg);
                await MessageBox.Show(msg, "Error reading file.");
                return;
            }

            SetLines(lines);
            CheckListType(false);
            IsBusy = false;
        }

        private async void OpenFileFromURL(Window owner)
        {
            Regex validation = new Regex(@"https?:\/\/\w+\.\w.+", RegexOptions.Compiled);
            var input = new TextInputDialog("Open File from URL", "URL", validation);
            if (await input.ShowDialog<ButtonResult>(owner) == ButtonResult.Ok)
            {
                // TODO
            }
        }

        private async void SetLines(IEnumerable<string> lines)
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

            Regex userlistRegex = new Regex(@"^\w{2,}$", RegexOptions.Compiled);
            Regex readfileRegex = new Regex(@"^(\.|\/\w+) (\w{2,}) ?(.+)?$", RegexOptions.Compiled);

            List<Entry> entryList = new List<Entry>();

            // display.
            bool warning = false;
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
                    warning = true;
                    LogViewModel.Log("WARNING: Could not parse line" + row);
                    //TODO What to accutally do here?
                }
            }

            Entries = new ObservableCollection<Entry>(entryList);
            if (warning)
                await MessageBox.Show("Could not parse one or more lines of the file, check the log", "Warning");
        }


        private async Task<bool> FilterEntriesForSpecialUsers()
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

            if (res && ProtectSpecialUsers && ProtectedUserMode_Cancel)
            {
                await MessageBox.Show("Protected user in list detected", "Action Aborted");
                return true;
            }

            if (res && ProtectSpecialUsers && ProtectedUserMode_Skip)
            {
                foreach (Entry entry in toRemove)
                {
                    Entries.Remove(entry);
                }

                RaisePropertyChanged(nameof(Entries));
                await MessageBox.Show("Protected user in list detected, entries removed",
                    "Protected user removed from list");
            }

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

        private async void CheckForProtectedUser()
        {
            await FilterEntriesForSpecialUsers();
        }

        private Task CreateWorkerTask(WorkingMode mode)
        {
            if (_twitchChatClient == null)
            {
                throw new ArgumentException();
            }

            CheckForProtectedUser();

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;


            return Task.Factory.StartNew(async () =>
                {
                    string TextReason = Reason;
                    for (int i = 0; i < Entries.Count; i++)
                    {
                        while (Paused)
                        {
                            await Task.Delay(1000, _token);
                        }

                        if (_token.IsCancellationRequested)
                        {
                            break;
                        }

                        Entry entry = Entries[i];
                        string commandtoExecute = String.Empty;

                        switch (mode)
                        {
                            case WorkingMode.Ban:
                            {
                                commandtoExecute = $"/ban {entry.Name} {TextReason}";
                                break;
                            }
                            case WorkingMode.Unban:
                            {
                                commandtoExecute = $"/unban {entry.Name}";
                                break;
                            }
                            case WorkingMode.Readfile:
                            {
                                commandtoExecute = entry.ChatCommand;
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }

                        foreach (var channel in _twitchChatClient.client.JoinedChannels)
                        {
#if DEBUG
                            Trace.WriteLine($"DEBUG #{channel.Channel}: PRIVMSG {commandtoExecute}");
#else
                        twitchChatClient.client.SendMessage(channel, commandtoExecute);
#endif
                            await Task.Delay(TimeSpan.FromMilliseconds(_messageDelay), _token);
                        }

                        BanProgress = (i + 1 / Entries.Count);
                        ETA = TimeSpan.FromMilliseconds((Entries.Count - i + 1) * channels.Count * _messageDelay);

                        entry.RowBackColor = "Green";
                    }
                },
                _token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).Result;
        }
    }
}