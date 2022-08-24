using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData;
using IX.Observable;
using MassBanToolMP.Helper;
using MassBanToolMP.Models;
using MassBanToolMP.Views;
using MassBanToolMP.Views.Dialogs;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using TwitchLib.Client.Events;

namespace MassBanToolMP.ViewModels;

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
    private const string URL_TMI = "https://twitchapps.com/tmi/";

    private const string QUESTION_LISTTYPEMISMATCHRUN =
        "Listtype does not match the selected operation mode. Do you want to ignore the additional params and run this anyways?";

    private const string LSITTYPEMISMATCH = "Listtype Mismatch";

    private readonly LogViewModel _logModel;

    private static IDisposable isConnectedObservable;

    private string _allowedActions;
    private string _oAuth;
    private string _reason;
    private string _username;
    private string _channelS = string.Empty;
    private string filterRegex = string.Empty;

    private double _banProgress;

    private ObservableCollection<Entry> _entries;
    private TimeSpan _eta;
    private int _messageDelay = 301;

    private bool _paused;
    private bool _dryRun;
    private bool _readFileCommandMismatchSkip;
    private bool _protectSpecialUsers = true;
    private bool _readFileCommandMismatchCancel = true;
    private bool _listFilterRemoveMatching = false;

    private Task _worker;
    private List<string> channels = new();
    private ContextMenu? _lastVisitedChannelsMenu;
    private List<string> _lastVisitedChannels = new();
    private ListType _listType;
    private CancellationToken _token;
    private LogWindow _logWindow;
    private CancellationTokenSource _tokenSource;
    private TwitchChatClient? _twitchChatClient;
    private Mutex _userMutex;


    public MainWindowViewModel()
    {
        _logModel = new LogViewModel();
        LogViewModel.Log("Init GUI...");

        WindowTitle = "MassBanTool " + Program.Version;
        Entries = new ObservableCollection<Entry>();

        OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
        EditLastVisitChannelCommand = ReactiveCommand.Create<Window>(EditLastVisitChannelsList);

        OpenFileFromURLCommand = ReactiveCommand.Create<Window>(OpenFileFromURL);
        FetchLastFollowersForChannelCommand = ReactiveCommand.Create<Window>(FetchLastFollowersFromChannel);
        ConnectCommand = ReactiveCommand.Create(Connect);
        SaveDataCommand = ReactiveCommand.Create(SaveData);
        LoadCredentialsCommand = ReactiveCommand.Create(LoadCredentials);
        StoreCredentialsCommand = ReactiveCommand.Create(StoreCredentials,
            this.WhenAnyValue(x => x.Username, x => x.OAuth,
                (userName, password) => !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password)));
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
        GetOAuthCommand = ReactiveCommand.Create(() => OpenUrl(URL_TMI));
        OpenWikiCommand = ReactiveCommand.Create(() => OpenUrl(HELP_URL_WIKI));
        CooldownInfoCommand = ReactiveCommand.Create(() => OpenUrl(HELP_URL_COOLDOWN));
        OpenRegexDocsCommand = ReactiveCommand.Create(() => OpenUrl(HELP_URL_REGEX_MS_DOCS));
        OpenRegex101Command = ReactiveCommand.Create(() => OpenUrl(HELP_URL_REGEX101));
        OpenGitHubPageCommand = ReactiveCommand.Create(() => OpenUrl(HELP_URL_MAIN_GITHUB));
        ShowLogWindowCommand = ReactiveCommand.Create<Window>(ShowLogWindow);

        LoadData();
        _listType = default;
        LogViewModel.Log("Done Init GUI...");
    }

    private async void EditLastVisitChannelsList(Window window)
    {
        var inputVM = new EditIENumerableDialogViewModel("Channels", _lastVisitedChannels);
        var input = new EditIeNumerableDialog()
        {
            DataContext = inputVM
        };
        if (await input.ShowDialog<ButtonResult>(window) == ButtonResult.Ok)
        {
            _lastVisitedChannels = inputVM.Objects.Select(x => x.Value).ToList();
            BuildLastVisitChannelContextMenu();
        }
    }


    private void StoreCredentials()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(OAuth))
            return;
        SecretHelper.StoreCredentials(Username, OAuth);
    }

    private Task Worker
    {
        get => _worker;
        set
        {
            if (SetProperty(ref _worker, value))
            {
                value.ContinueWith((task) =>
                {
                    RaisePropertyChanged(nameof(CanExecRun));
                    RaisePropertyChanged(nameof(CanExecPauseAbort));
                });
                RaisePropertyChanged(nameof(CanExecPauseAbort));
                RaisePropertyChanged(nameof(CanExecRun));
            }
        }
    }

    public bool CanConnect
    {
        get
        {
            var hasErrors = false;
            var errs = GetErrors(nameof(ChannelS));
            if (errs is List<string> errorList) hasErrors = errorList.Count > 0;

            return !string.IsNullOrEmpty(Username)
                   && !string.IsNullOrEmpty(OAuth)
                   && channels.Count > 0
                   && !hasErrors;
        }
    }

    public DataGrid DataGrid { get; set; }

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
            if (int.TryParse(value, out var val))
                ClearError(nameof(MessageDelay), MESSAGE_DELAY_INVALID_TYPE);
            else
                AddError(nameof(MessageDelay), MESSAGE_DELAY_INVALID_TYPE);

            if (val < 300)
            {
                AddError(nameof(MessageDelay), MESSAGE_DELAY_TOO_LOW);
            }
            else
            {
                ClearError(nameof(MessageDelay), MESSAGE_DELAY_TOO_LOW);
                SetProperty(ref _messageDelay, val);
            }
        }
    }

    public string ChannelS
    {
        get => _channelS;
        set
        {
            var cache = value.Split(",");

            if (cache.Any(x => x.Trim().Length < 3))
                AddError(nameof(ChannelS), CHANNEL_INVALID);
            else
                ClearError(nameof(ChannelS));

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
                return _twitchChatClient.client.IsConnected && _twitchChatClient.client.JoinedChannels.Any();

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
            if (IsConnected) return "Switch Channel(s)";

            return "Connect";
        }
    }

    public ListType ListType
    {
        get => _listType;
        set => SetProperty(ref _listType, value);
    }

    private ReactiveCommand<Window, Unit> ExitCommand { get; }
    private ReactiveCommand<Window, Unit> OpenFileCommand { get; }
    private ReactiveCommand<Window, Unit> OpenFileFromURLCommand { get; }
    private ReactiveCommand<Window, Unit> FetchLastFollowersForChannelCommand { get; }
    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadCredentialsCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveDataCommand { get; }
    public ReactiveCommand<Unit, Unit> StoreCredentialsCommand { get; }
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
    public ReactiveCommand<Window, Unit> EditLastVisitChannelCommand { get; }
    public ReactiveCommand<Unit, Unit> GetOAuthCommand { get; }

    public ObservableCollection<Entry> Entries
    {
        get => _entries;
        set
        {
            if (SetProperty(ref _entries, value)) RaisePropertyChanged(nameof(CanExecRun));
        }
    }

    public string ReadFileAllowedActions
    {
        get => _allowedActions;
        set => SetProperty(ref _allowedActions, value);
    }

    public Dictionary<string, List<string>> ChannelModerators { get; private set; } = new();

    public Dictionary<string, List<string>> ChannelVIPs { get; private set; } = new();

    public List<string> allSpecialChannelUser
    {
        get
        {
            var result = new List<string>();
            foreach (var elm in ChannelModerators) result.AddRange(elm.Value);

            foreach (var elm in ChannelVIPs) result.AddRange(elm.Value);

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

    public ContextMenu? LastVisitedChannelsMenu => _lastVisitedChannelsMenu;

    public bool DryRun
    {
        get => _dryRun;
        set => SetProperty(ref _dryRun, value);
    }

    public bool ReadFileCommandMismatch_Cancel
    {
        get => _readFileCommandMismatchCancel;
        set => SetProperty(ref _readFileCommandMismatchCancel, value);
    }

    public bool ReadFileCommandMismatch_Skip
    {
        get => _readFileCommandMismatchSkip;
        set => SetProperty(ref _readFileCommandMismatchSkip, value);
    }

    public string WindowTitle { get; set; }


    private void LoadData()
    {
        LogViewModel.Log("Try loading setting for this User.");
        var fileName = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");

        var filecontent = string.Empty;
        DataWrapper data = null;

        if (File.Exists(fileName))
            try
            {
                filecontent = File.ReadAllText(fileName);
                filecontent = filecontent.Trim();
                data = DataWrapper.fromJson(filecontent);
            }
            catch (Exception e)
            {
                LogViewModel.Log("Something went wrong loading setting for this User. - " + e.Message);
            }

        if (data != null && data.message_delay != default) MessageDelay = data.message_delay.ToString();

        if (data?.lastVisitedChannel != null)
        {
            _lastVisitedChannels = data.lastVisitedChannel.ToList();


            BuildLastVisitChannelContextMenu();
        }
        else
        {
            _lastVisitedChannelsMenu = null;
        }

        if (data?.AllowedActions != null)
            ReadFileAllowedActions = string.Join(Environment.NewLine, data.AllowedActions);
        else
            ReadFileAllowedActions = string.Join(Environment.NewLine, Defaults.AllowedActions);

        LogViewModel.Log("Done loading setting for this User.");
    }

    private void BuildLastVisitChannelContextMenu()
    {
        _lastVisitedChannelsMenu = new ContextMenu();

        MenuItem item;
        var items = new List<MenuItem>();
        foreach (var s in _lastVisitedChannels)
        {
            var header = s.Replace("_", "__");
            item = new MenuItem()
            {
                Header = header,
                DataContext = s
            };
            item.Click += delegate(object? sender, RoutedEventArgs args)
            {
                if (sender is MenuItem menuitem) ChannelS = (string)menuitem.DataContext;
            };
            items.Add(item);
        }

        _lastVisitedChannelsMenu.Items = items;
        RaisePropertyChanged(nameof(LastVisitedChannelsMenu));
    }

    private void SaveData()
    {
        var fileName = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");
        var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MassBanTool");

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        if (!File.Exists(fileName)) File.Create(fileName).Close();

        var data = new DataWrapper()
        {
            lastVisitedChannel = new HashSet<string>(_lastVisitedChannels),
            AllowedActions = _allowedActions.Split(Environment.NewLine)
                .Select(x => x.Trim())
                .Where(x => x != string.Empty)
                .ToHashSet(),
            message_delay = _messageDelay
        };

        var result = data.toJSON();


        File.WriteAllText(fileName, result);
    }

    private void RaiseIsConnectedChanged()
    {
        RaisePropertyChanged(nameof(IsConnected));
        RaisePropertyChanged(nameof(ConButtonText));
        RaisePropertyChanged(nameof(CanExecRun));
    }

    private void OpenUrl(string url)
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
                //ERROR
                return;

            var actions = ReadFileAllowedActions.Split(Environment.NewLine);

            var toRemove = Entries.AsParallel().Where(x => !actions.Contains(x.Command)).ToList();

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
        var listEnumerator = 0;
        foreach (var entry in Entries)
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
                    LogViewModel.Log(
                        $"INFO: Line {listEnumerator} -> '{entry.ChatCommand}' --- triggered Listtype Mixed");
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
                LogViewModel.Log(
                    $"INFO: Line {listEnumerator} -> '{entry.ChatCommand}' --- triggered Listtype Malformed");
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

        var toRemove = Entries.AsParallel().Where(x =>
        {
            if (regex.IsMatch(x.ChatCommand)) return !_listFilterRemoveMatching;

            return _listFilterRemoveMatching;
        }).ToList();

        Entries.RemoveMany(toRemove);
    }

    private async void ExecReadFile()
    {
        if (!CanExecRun) return;

        if (ListType != ListType.ReadFile)
        {
            await MessageBox.Show("List is not a Readfile, cannot execute Readfile opon it.", LSITTYPEMISMATCH);
            return;
        }

        if (!await FilterEntriesForSpecialUsers()) return;
        if (!await CheckReadlistForIllegalCommands()) return;
        Worker = CreateWorkerTask(WorkingMode.Readfile);
    }

    private async void ExecUnban()
    {
        if (!CanExecRun) return;

        if (ListType != ListType.UserList)
            if (await MessageBox.Show(
                    QUESTION_LISTTYPEMISMATCHRUN,
                    LSITTYPEMISMATCH,
                    ButtonEnum.YesNo) != ButtonResult.Yes)
                return;

        Worker = CreateWorkerTask(WorkingMode.Unban);
    }

    private async void ExecBan()
    {
        if (!CanExecRun) return;

        if (ListType != ListType.UserList)
            if (await MessageBox.Show(
                    QUESTION_LISTTYPEMISMATCHRUN,
                    LSITTYPEMISMATCH,
                    ButtonEnum.YesNo) != ButtonResult.Yes)
                return;

        if (await FilterEntriesForSpecialUsers()) Worker = CreateWorkerTask(WorkingMode.Ban);
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
            if (cred == null) return;
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
            {
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

    private Regex CreateFilterRegex()
    {
        var regexOptions = RegexOptions.Compiled;

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
        if (DataGrid.Columns.Any(x => x.Header as string == eChannel)) return;

        var col = new DataGridTextColumn()
        {
            Header = eChannel,
            Binding = new Binding() { Path = $"Result[{eChannel}]", Priority = BindingPriority.Animation },
            IsReadOnly = false
        };

        DataGrid.Columns.Add(col);
    }

    private void RemoveChannelFromGrid(string eChannel)
    {
        var toremove = DataGrid.Columns.Skip(2).FirstOrDefault(x => x.Header as string == eChannel);
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
        if (!CanConnect) return;

        if (IsConnected) SwitchChannel();


        if (!CheckMutex())
        {
            await MessageBox.Show("Only one instance with the same username allowed to run.", "Mutex error");
            return;
        }

        _twitchChatClient = new TwitchChatClient(this, _username, _oAuth, channels);

        _twitchChatClient.client.OnIncorrectLogin += IncorrectLogin;
        _twitchChatClient.client.OnVIPsReceived += Client_OnVIPsReceived;
        _twitchChatClient.client.OnModeratorsReceived += Client_OnModeratorsReceived;
        _twitchChatClient.client.OnMessageThrottled += Client_OnMessageThrottled;
        _twitchChatClient.client.OnConnected += ClientOnConnected;
        _twitchChatClient.client.OnJoinedChannel += ClientOnOnJoinedChannel;
        _twitchChatClient.client.OnLeftChannel += Client_OnLeftChannel;

        if (!_twitchChatClient.client.IsConnected) await MessageBox.Show("Failed to connect to twitch", "Warning");

        isConnectedObservable = _twitchChatClient.WhenAnyValue(x => x.client.IsConnected)
            .Subscribe(_ => RaisePropertyChanged(nameof(IsConnected)));
    }

    private void Client_OnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            RemoveChannelFromGrid(e.Channel);
        }
        else
        {
            var tsk = Dispatcher.UIThread.InvokeAsync(() => RemoveChannelFromGrid(e.Channel));
            tsk.Wait();
        }
    }

    private void ClientOnOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            AddChannelToGrid(e.Channel);
        }
        else
        {
            var tsk = Dispatcher.UIThread.InvokeAsync(() => AddChannelToGrid(e.Channel));
            tsk.Wait();
        }
    }

    private void ClientOnConnected(object? sender, OnConnectedArgs e)
    {
        RaiseIsConnectedChanged();
    }

    private void SwitchChannel()
    {
        if (_twitchChatClient == null)
        {
            LogViewModel.Log(
                "Application reached an impossible state. No Twitch Client and trying to switch channels.");
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
        }

        LogViewModel.Log("Joining Channels...");
        foreach (var channel in tojoin) _twitchChatClient.client.JoinChannel(channel, true);
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
        var toRemove =
            channels.First(x => x.Equals(userStateChannel, StringComparison.CurrentCultureIgnoreCase));
        channels.Remove(toRemove);

        _channelS = string.Join(", ", channels);
        RaisePropertyChanged(nameof(ChannelS));

        await MessageBox.Show("Channel: " + userStateChannel + Environment.NewLine + "Disconnecting.",
            "Missing Permissions in channel.");
    }

    public void OnUserBanned(string channel, string username)
    {
        AddToResult(username, channel, "Banned");
    }

    public void OnUserAlreadyBanned(string channel, string username)
    {
        AddToResult(username, channel, "Already Banned");
    }

    public void OnBadUserBan(string channel, string username, string msg_id)
    {
        AddToResult(username, channel, "Bad Ban Target - " + msg_id);

        var entry = Entries.FirstOrDefault(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
        if (entry != null)
            entry.IsValid = false;
    }

    private void AddToResult(string username, string channel, string res)
    {
        var item = Entries.FirstOrDefault(x =>
            x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));

        if (!item.Result.ContainsKey(channel))
        {
            var c = item.Result;
            c[channel] = res;
            item.Result = new ConcurrentObservableDictionary<string, string>(c);
        }
        else
        {
            item.Result[channel] = res;
        }
    }

    private async void HandleAddEntry(Window window)
    {
        var diag = new NewEntryView();
        await diag.ShowDialog(window);

        if (!diag.result)
            return;

        var name = diag.Username.Trim();

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
            var items = new List<Entry>();
            for (var i = 0; i < item.Count; i++)
                if (item[i] is Entry entry)
                    items.Add(entry);

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
        var diag = new OpenFileDialog();
        diag.AllowMultiple = false;
        diag.Title = "Open File:";
        var path = await diag.ShowAsync(window);

        IsBusy = true;

        if (path == null || path.Length == 0)
        {
            IsBusy = false;
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
            var msg = "Error while opening file:\n" + e.Message;
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
        var validation = new Regex(@"https?:\/\/\w+\.\w.+", RegexOptions.Compiled);
        var input = new TextInputDialog("Open File from URL", "URL", validation);
        if (await input.ShowDialog<ButtonResult>(owner) == ButtonResult.Ok)
        {
            IsBusy = true;
            Uri uri;
            try
            {
                uri = new Uri(input.BoxContent, UriKind.Absolute);
            }
            catch (UriFormatException)
            {
                await MessageBox.Show("Invalid URL", "Error");
                return;
            }

            FetchLinesAndSet(uri);
            IsBusy = false;
        }
    }

    private async void FetchLastFollowersFromChannel(Window owner)
    {
        var validation = new Regex(@"\w{2,}", RegexOptions.Compiled);
        var input = new TextInputDialog("Channel to query", "Channel", validation);

        string URL = "https://cactus.tools/twitch/followers";

        if (await input.ShowDialog<ButtonResult>(owner) == ButtonResult.Ok)
        {
            IsBusy = true;

            string urlParameters = $"?channel={input.BoxContent}&max=1000";
            Uri uri = new Uri(URL + urlParameters);

            FetchLinesAndSet(uri);

            IsBusy = false;
        }
    }


    private async void FetchLinesAndSet(Uri uri)
    {
        HttpResponseMessage response;

        using (HttpClient client = new HttpClient())
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Headers =
                {
                    { HttpRequestHeader.Accept.ToString(), "text/plain" },
                    { HttpRequestHeader.UserAgent.ToString(), "MassBanTool/" + Program.Version }
                }
            };

            response = await client.SendAsync(httpRequestMessage);
        }

        if (!response.IsSuccessStatusCode)
        {
            await MessageBox.Show(
                response.ReasonPhrase ?? "Server did not give a reason. For status code: " + response.StatusCode,
                "Error while fetching site");
            return;
        }

        string content = await response.Content.ReadAsStringAsync();

        string[] lines = content.Split(Environment.NewLine).AsParallel().Select(x => x.Trim()).ToArray();

        SetLines(lines);
        CheckListType(false);
    }


    private async void SetLines(IEnumerable<string> lines)
    {
        var rows = new List<string>();
        // iterate over each line cleaning

        var raw = string.Empty;
        foreach (var line in lines)
        {
            raw = line.Trim();
            if (!string.IsNullOrEmpty(raw)) rows.Add(raw);
        }

        var userlistRegex = new Regex(@"^\w{2,}$", RegexOptions.Compiled);
        var readfileRegex = new Regex(@"^((?:\.|\/)\w+) (\w{2,}) ?(.+)?$", RegexOptions.Compiled);

        var entryList = new List<Entry>();

        // display.
        var warning = false;
        foreach (var row in rows)
        {
            var isUser = userlistRegex.IsMatch(row);
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
        var toRemove = new List<Entry>();
        foreach (var entry in Entries)
            if (allSpecialChannelUser.Any(x =>
                    string.Equals(x, entry.Name, StringComparison.CurrentCultureIgnoreCase)))
                toRemove.Add(entry);
        var res = toRemove.Any();
        if (res && ProtectSpecialUsers && ProtectedUserMode_Cancel)
        {
            await MessageBox.Show("Protected user in list detected", "Action Aborted");
            return false;
        }

        if (res && ProtectSpecialUsers && ProtectedUserMode_Skip)
        {
            foreach (var entry in toRemove) Entries.Remove(entry);
            RaisePropertyChanged(nameof(Entries));
        }

        return true;
    }

    private async Task<bool> CheckReadlistForIllegalCommands()
    {
        var allowedActions = _allowedActions.Split(Environment.NewLine);

        if (allowedActions.Length == 0)
        {
            await MessageBox.Show("No allowed Action", "Readfile Warning");
            return false;
        }

        var query = Entries.AsParallel().Where(x =>
            !allowedActions.Contains(x.Command[1..], StringComparer.Create(CultureInfo.CurrentCulture, true)));

        if (query.Any())
        {
            if (_readFileCommandMismatchSkip)
            {
                LogViewModel.Log(
                    $"Removing {query.Count()} Entries since they do not match the readfile allowed actions.");
                Entries.RemoveMany(query.ToList());
                RaisePropertyChanged(nameof(Entries));
                return Entries.Count > 0;
            }
            else if (_readFileCommandMismatchCancel)
            {
                await MessageBox.Show("Mismatch between allowed commands and commands used in the file.", "Warning");
                return false;
            }
        }

        return true;
    }

    private Task CreateWorkerTask(WorkingMode mode)
    {
        if (_twitchChatClient == null) throw new ArgumentException();

        _tokenSource = new CancellationTokenSource();
        _token = _tokenSource.Token;

        foreach (Entry entry in _entries)
        foreach (string channel in channels)
        {
            if (!entry.Result.ContainsKey(channel) || entry.Result[channel] == null)
                entry.Result[channel] = string.Empty;
            entry.Result = new ConcurrentObservableDictionary<string, string>(entry.Result);
        }

        return Task.Factory.StartNew(async () =>
            {
                var TextReason = Reason;
                for (var i = 0; i < Entries.Count; i++)
                {
                    while (Paused) await Task.Delay(1000, _token);

                    if (_token.IsCancellationRequested) break;

                    var entry = Entries[i];
                    var commandtoExecute = string.Empty;
                    string user = string.Empty;

                    switch (mode)
                    {
                        case WorkingMode.Ban:
                        {
                            commandtoExecute = $"/ban {entry.Name} {TextReason}".Trim();
                            user = entry.Name;
                            break;
                        }
                        case WorkingMode.Unban:
                        {
                            commandtoExecute = $"/unban {entry.Name}";
                            user = entry.Name;
                            break;
                        }
                        case WorkingMode.Readfile:
                        {
                            commandtoExecute = entry.ChatCommand;
                            user = entry.Name;
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }

                    foreach (var channel in _twitchChatClient.client.JoinedChannels)
                    {
                        if (!entry.IsValid) break;

                        if (entry.Result.ContainsKey(channel.Channel.ToLower()) &&
                            !string.IsNullOrEmpty(entry.Result[channel.Channel.ToLower()])) continue;

                        if (DryRun)
                        {
                            LogViewModel.Log($"DEBUG #{channel.Channel}: PRIVMSG {commandtoExecute}");
                            OnUserBanned(channel.Channel, user);
                        }
                        else
                        {
                            _twitchChatClient.SendMessage(channel, commandtoExecute);
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(_messageDelay), _token);
                    }

                    BanProgress = (i + 1) / (double)Entries.Count * 100;
                    ETA = TimeSpan.FromMilliseconds((Entries.Count - i + 1) * channels.Count * _messageDelay);

                    entry.RowBackColor = "Green";
                }

                ETA = TimeSpan.Zero;
                BanProgress = 100;
            },
            _token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Result;
    }

    public async void FailedToJoinChannel(string exceptionChannel)
    {
        await MessageBox.Show("Failed to join channel " + exceptionChannel, "Warning");
        RemoveChannelFromGrid(exceptionChannel);
    }
}