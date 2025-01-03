#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData;
using IX.Library.Collections;
using MassBanToolMP.Helper;
using MassBanToolMP.Models;
using MassBanToolMP.Views;
using MassBanToolMP.Views.Dialogs;
using MsBox.Avalonia.Enums;
using ReactiveUI;
#endregion

namespace MassBanToolMP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region Constructor

        public MainWindowViewModel()
        {
            LogViewModel.Log("Init GUI...");

            WindowTitle = "MassBanTool " + Program.Version;

            Entries = new ObservableCollection<Entry>();

            OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
            EditLastVisitChannelCommand = ReactiveCommand.Create<Window>(EditLastVisitChannelsList);

            OpenFileFromURLCommand = ReactiveCommand.Create<Window>(OpenFileFromURL);
            FetchLastFollowersForChannelCommand = ReactiveCommand.Create<Window>(FetchLastFollowersFromChannel);

            var canSaveObservable = this.WhenAnyValue(x => x.Entries, entries => entries.Any());
            SaveListAsCommand =
                ReactiveCommand.Create<Window>(window => SaveListAs(window, WorkingMode.Readfile), canSaveObservable);
            SaveBanListAsCommand =
                ReactiveCommand.Create<Window>(window => SaveListAs(window, WorkingMode.Ban), canSaveObservable);
            SaveUnBanListAsCommand =
                ReactiveCommand.Create<Window>(window => SaveListAs(window, WorkingMode.Unban), canSaveObservable);

            CheckForNewVersionsCommand = ReactiveCommand.Create<Window>(CheckForNewVersions);
            ClearResultsCommand = ReactiveCommand.Create(ClearResults);
            ConnectCommand = ReactiveCommand.Create(Connect);
            SaveDataCommand = ReactiveCommand.Create(SaveData);
            LoadCredentialsCommand = ReactiveCommand.Create(LoadCredentials);
            StoreCredentialsCommand = ReactiveCommand.Create(StoreCredentials,
                this.WhenAnyValue(x => x.Username, x => x.OAuth,
                    (userName, password) => !string.IsNullOrEmpty(password)));
            OnClickPropertiesAddEntry = ReactiveCommand.Create<Window>(HandleAddEntry);
            OnClickPropertiesPasteClipboard = ReactiveCommand.Create<Window>(HandlePasteEntries);
            OnDataGridRemoveEntry = ReactiveCommand.Create<object>(RemoveEntry);
            OnClickPauseActionCommand = ReactiveCommand.Create(PauseAction);
            OnClickCancelActionCommand = ReactiveCommand.Create(CancelAction);
            RunBanCommand = ReactiveCommand.Create(ExecBan);
            RunUnbanCommand = ReactiveCommand.Create(ExecUnban);
            RunReadfileCommand = ReactiveCommand.Create(ExecReadFile);
            RunListFilterCommand = ReactiveCommand.Create(ExecListFilter);
            RunCheckListTypeCommand = ReactiveCommand.Create(() => CheckListType(true));
            RunSortListCommand = ReactiveCommand.Create(SortList);
            RunRemoveNotAllowedActionsCommand = ReactiveCommand.Create(RemoveNotAllowedActions);
            GetOAuthCommand = ReactiveCommand.Create<Window>(window => GetAccessToken(window));
            OpenWikiCommand = ReactiveCommand.Create(() => HttpHelper.OpenUrl(HELP_URL_WIKI));
            CooldownInfoCommand = ReactiveCommand.Create(() => HttpHelper.OpenUrl(HELP_URL_COOLDOWN));
            OpenRegexDocsCommand = ReactiveCommand.Create(() => HttpHelper.OpenUrl(HELP_URL_REGEX_MS_DOCS));
            OpenRegex101Command = ReactiveCommand.Create(() => HttpHelper.OpenUrl(HELP_URL_REGEX101));
            OpenGitHubPageCommand = ReactiveCommand.Create(() => HttpHelper.OpenUrl(HELP_URL_MAIN_GITHUB));
            ShowLogWindowCommand = ReactiveCommand.Create<Window>(ShowLogWindow);
            ShowInfoWindowCommand = ReactiveCommand.Create<Window>(ShowInfoWindow);
            QueryUsersInListCommand = ReactiveCommand.Create<Unit, Task>(unit => QueryIdsForEntries());
            EditSpecialUsersCommand = ReactiveCommand.Create(EditSpecialUsers);
            TokenInfoCommand = ReactiveCommand.Create<MainWindow>(ShowTokenInfo);

            ApplySettings();
            _listType = default;
            SpecialUsers = new List<string>(); // for now.
            LogViewModel.Log("Done Init GUI...");
        }

        #endregion

        private void RaiseIsConnectedChanged()
        {
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(ConButtonText));
            RaisePropertyChanged(nameof(CanExecRun));
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
            if (!await ValidateScopeForAction(WorkingMode.Readfile)) return;

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

            if (!await ValidateScopeForAction(WorkingMode.Unban)) return;

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

            if (!await ValidateScopeForAction(WorkingMode.Ban)) return;

            if (await FilterEntriesForSpecialUsers())
            {
                Worker = CreateWorkerTask(WorkingMode.Ban);
            }
        }

        private void CancelAction()
        {
            _tokenSource.Cancel();
            try
            {
                Worker.Wait(2000);
            }
            catch (AggregateException)
            {
            }

            ETA = TimeSpan.Zero;

            try
            {
                Worker.Dispose();
            }
            catch (Exception e)
            { 
                // ignored
            }

            IsBusy = false;
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

                var m = Regex.Match(cred, @"^(?:oauth:|Bearer )?(.+)$", RegexOptions.IgnoreCase);
                string token = cred;
                if (m.Success)
                {
                    token = m.Groups[1].Value;
                }

                Program.API.Settings.AccessToken = "Bearer " + token;
                OAuth = Program.API.Settings.AccessToken;
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
                DataContext = LogViewModel.Instance
            };
            _logWindow.Closed += (sender, args) =>
            {
                _logWindow = null;
                GC.Collect();
            };
            _logWindow.Show(owner);
        }

        private async void ShowInfoWindow(Window owner)
        {
            await MessageBox.Show("Author: Dr_SFFan123\n" +
                                  "QA: Sileniful\n" +
                                  "License: MIT\n" +
                                  "Version: " + Program.Version + "\n" +
                                  "Git Repo: https://github.com/SFFan123/MassBanTool", "MassBanTool Info");
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


        private void RemoveChannelFromGrid(string eChannel)
        {
            var toremove = DataGrid.Columns.Skip(3).FirstOrDefault(x => x.Header as string == eChannel);
            DataGrid.Columns.Remove(toremove);
        }

        private void RemoveAllChannelsFromGrid()
        {
            var toremove = DataGrid.Columns.Skip(3);
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

            if (!OAuth.StartsWith("Bearer"))
            {
                OAuth = "Bearer " + OAuth;
            }

            Program.API.Settings.AccessToken = OAuth;

            GetTokenRateLimit(out var limit, out var span, out _);

            if(limit != -1 && span != TimeSpan.Zero)
                Program.CreateApiClient(limit, span);

            Program.API.Settings.AccessToken = OAuth;

            if (IsConnected)
            {
                SwitchChannel();
                return;
            }

            IsBusy = true;

            try
            {
                var res = await Program.API.Auth.ValidateAccessTokenAsync(Program.API.Settings.AccessToken);
                TokenScopes = res.Scopes;
                _userId = res.UserId;
                _IsConnected = true;
            }
            catch (Exception e)
            {
                LogViewModel.Log(e.Message);
                await MessageBox.Show("Could not verify token", "Token error");
                IsBusy = false;
                return;
            }

            await GetChannelIds();

            channelIDs.ForEach((x) => { AddChannelToGrid(x.Key); });

            if (!_lastVisitedChannels.Contains(ChannelS))
                _lastVisitedChannels.Add(ChannelS);

            RaiseIsConnectedChanged();

            IsBusy = false;
        }

        public void AddChannelToGrid(string channelName)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _AddChannelToGrid(channelName);
            }
            else
            {
                var tsk = Dispatcher.UIThread.InvokeAsync(() => _AddChannelToGrid(channelName));
                tsk.Wait();
            }
        }

        private void _AddChannelToGrid(string channelName)
        {
            if (!SettingShowResult)
            {
                return;
            }

            if (DataGrid.Columns.Any(x => x.Header as string == channelName)) return;

            var col = new DataGridTextColumn()
            {
                Header = channelName,
                Binding = new Binding()
                    { Path = $"Result[{channelName}]", Priority = BindingPriority.Style, Mode = BindingMode.OneWay, TargetNullValue = string.Empty},
                IsReadOnly = true,
            };

            DataGrid.Columns.Add(col);

            if (!channels.Except(DataGrid.Columns.Skip(3).Select(x => x.Header as string)).Any())
            {
                IsBusy = false;
            }
        }


        private async void SwitchChannel()
        {
            var joinedChannels = channelIDs.Select(x => x.Key.ToLower());
            var toleave = joinedChannels.Except(channels).ToList();
            toleave.ForEach(x => channelIDs.Remove(x));
            toleave.ForEach(RemoveChannelFromGrid);
            toleave = null;

            var lst = DataGrid.Columns.Skip(2).Select(x => x.Header as string).ToList();
            var tojoin = channels.Except(lst).ToList();
            tojoin.ForEach(AddChannelToGrid);
            tojoin = null;

            await GetChannelIds();

            if (!_lastVisitedChannels.Contains(ChannelS))
                _lastVisitedChannels.Add(ChannelS);

            GC.Collect();
        }

        public void OnUserBanned(string channel, string username, bool dryRun = false)
        {
            AddToResult(username, channel, dryRun ? ResultDryBanned : ResultBanned);
        }

        public void OnUserAlreadyBanned(string channel, string username)
        {
            AddToResult(username, channel, ResultAlreadyBanned);
        }

        public void OnInvalidBanTarget(string channel, string username)
        {
            AddToResult(username, channel, InvalidBanTarget);
        }


        private void AddToResult(string username, string channel, string res)
        {
            var item = Entries.FirstOrDefault(x =>
                x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));

            if (!item.Result.ContainsKey(channel))
            {
                item.Result[channel] = res;
                if (SettingShowResult)
                {
                    item.Result.RefreshViewers();
                }
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

        private async void HandlePasteEntries(Window window)
        {
            var clipboard = window.Clipboard;
            if(clipboard == null)
                return;

            var text = await clipboard.GetTextAsync();

            if(string.IsNullOrEmpty(text))
                return;

            var lines = text.Split(Environment.NewLine);

            SetLines(lines);
            CheckListType(true);
        }

        private async void OpenFile(Window window)
        {
            var diag = new OpenFileDialog();
            diag.AllowMultiple = false;
            diag.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter() { Name = "Text file", Extensions = new List<string>() { "txt" } },
                new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } }
            };
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
            GC.Collect();
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
            var inputVM = new FetchLastFollowersFromAPIViewModel();
            var input = new FetchLastFollowersFromAPIDialog()
            {
                DataContext = inputVM
            };

            if (await input.ShowDialog<ButtonResult>(owner) == ButtonResult.Ok)
            {
                IsBusy = true;

                string urlParameters = $"?channel={inputVM.Channel}&max={inputVM.FetchAmount}";
                Uri uri = new Uri(API_CACTUSTOOL_LASTFOLLOWERS + urlParameters);

                FetchLinesAndSet(uri);

                IsBusy = false;
            }
        }

        private async void FetchLinesAndSet(Uri uri)
        {
            HttpResponseMessage response = await HttpHelper.FetchPlainPage(uri);

            if (!response.IsSuccessStatusCode)
            {
                await MessageBox.Show(
                    response.ReasonPhrase ?? "Server did not give a reason. For status code: " + response.StatusCode,
                    "Error while fetching site");
                return;
            }

            string content = await response.Content.ReadAsStringAsync();

            string[] lines = content.Split('\r', '\n')
                .AsParallel()
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            SetLines(lines);
            CheckListType(false);
        }

        private async void CheckForNewVersions(Window window)
        {
            IsBusy = true;

            var rel = await HttpHelper.FetchGitHubReleases();

            string version = string.Empty;
            string name = string.Empty;
            string url = string.Empty;

            if (_includePrereleases)
            {
                version = rel.Item2["tag_name"].ToString();
                name = rel.Item2["name"].ToString();
                url = rel.Item2["html_url"].ToString();
            }
            else
            {
                version = rel.Item1["tag_name"].ToString();
                name = rel.Item1["name"].ToString();
                url = rel.Item1["html_url"].ToString();
            }

            var match = Regex.Match(version, @"\d+\.\d+\.\d+\.\d+");

            Version remoteVersion;
            ButtonResult DiagResult = ButtonResult.Cancel;
            if (Version.TryParse(match.Value, out remoteVersion))
            {
                if (remoteVersion > Program.Version)
                {
                    DiagResult = await MessageBox.Show(
                        "New version.\n" + version + Environment.NewLine + name + "\nDo you want to open the page?",
                        "New Version",
                        ButtonEnum.YesNo);
                }
                else
                {
                    await MessageBox.Show("This is the newest Version", "No new version");
                }
            }
            else
            {
                DiagResult = await MessageBox.Show(
                    "Cannot parse new version.\n" + version + Environment.NewLine + name +
                    "\nDo you want to open the page?", "Cannot parse Version",
                    ButtonEnum.YesNo);
            }

            if (DiagResult == ButtonResult.Yes)
            {
                HttpHelper.OpenUrl(url);
            }

            IsBusy = false;
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
                    LogViewModel.Log("WARNING: Could not parse line '" + row + "' index: " + rows.IndexOf(row));
                    //TODO What to accutally do here?
                }
            }

            Entries = new ObservableCollection<Entry>(entryList);
            if (warning)
                await MessageBox.Show("Could not parse one or more lines of the file, check the log", "Warning");
        }
        

        private async Task<bool> FilterEntriesForSpecialUsers()
        {
           // read special user file

            if (File.Exists(SpecialUserFilePath))
            {
                using (var filestream = File.OpenText(SpecialUserFilePath))
                {
                    for (string? line = await filestream.ReadLineAsync();
                         line != null;
                         line = await filestream.ReadLineAsync())
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                            continue;

                        SpecialUsers.Add(line);
                    }
                }

                LogViewModel.Log($"Read {SpecialUsers.Count} from file.");
            }
            else
            {
                LogViewModel.Log("Warning: no protected users defined!");
            }

            var toRemove = new List<Entry>();
            foreach (var entry in Entries)
                if (SpecialUsers.Any(x =>
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
                    await MessageBox.Show("Mismatch between allowed commands and commands used in the file.",
                        "Warning");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateScopeForAction(WorkingMode mode)
        {
            switch (mode)
            {
                case WorkingMode.Ban:
                {
                    return (TokenScopes.Contains(ReadFileOperationScopeMapping
                        .OperationScopeMapping[ReadFileOperation.Ban]
                        .Item1));
                }
                case WorkingMode.Unban:
                {
                    return (TokenScopes.Contains(ReadFileOperationScopeMapping
                        .OperationScopeMapping[ReadFileOperation.UnBan]
                        .Item1));
                }
                case WorkingMode.Readfile:
                {
                    Entries.ParallelForEach(entry =>
                    {
                        try
                        {
                            entry.Operation = parseOperation(entry.Command);
                        }
                        catch
                        {
                            entry.IsValid = false;
                            // ignored
                        }
                    });

                    var query = Entries.AsParallel().Where(x => (TokenScopes.Contains(ReadFileOperationScopeMapping
                        .OperationScopeMapping[x.Operation]
                        .Item1)));

                    if (!query.Any())
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
                            await MessageBox.Show("Mismatch between allowed commands and commands used in the file.",
                                "Warning");
                            return false;
                        }
                    }

                    return true;
                }
                default:
                {
                    // do nothing
                    return false;
                }
            }
        }

        private ReadFileOperation parseOperation(string com)
        {
            if (com.StartsWith(".") || com.StartsWith("/"))
                com = com.Substring(1);

            return Enum.GetValues<ReadFileOperation>()
                .First(x => x.ToString().ToLowerInvariant() == com);
        }
        
        #region Properties

        public bool CanConnect
        {
            get
            {
                var hasErrors = false;
                var errs = GetErrors(nameof(ChannelS));
                if (errs is List<string> errorList) hasErrors = errorList.Count > 0;

                return !string.IsNullOrEmpty(OAuth)
                       && channels.Count > 0
                       && !hasErrors;
            }
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

                if (val < 0)
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
                channels = cache.Select(x => x.Trim()).ToList();
                RaisePropertyChanged(nameof(CanConnect));
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

        public ObservableCollection<Entry> Entries
        {
            get => _entries;
            set
            {
                if (SetProperty(ref _entries, value))
                    RaisePropertyChanged(nameof(CanExecRun));
            }
        }


        public bool CanExecPauseAbort => Worker != null && !Worker.IsCompleted;

        public bool CanExecRun => IsConnected && !string.IsNullOrEmpty(OAuth) && Entries.Count > 0 &&
                                  (Worker == null || Worker.IsCompleted);

        public string PauseButtonText => Paused ? "Resume" : "Pause";
        public string ConButtonText => IsConnected ? "Switch Channel(s)" : "Connect";

        public ContextMenu? LastVisitedChannelsMenu => _lastVisitedChannelsMenu;


        public string ToolStatus
        {
            get => _toolStatus;
            set => SetProperty(ref _toolStatus, value);
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

        public double BanProgress
        {
            get => _banProgress;
            set => SetProperty(ref _banProgress, value);
        }

        public bool SettingLoadCredentialsOnStartup
        {
            get => _settingLoadCredentialsOnStartup;
            set => SetProperty(ref _settingLoadCredentialsOnStartup, value);
        }

        public bool CheckForNewVerionOnStartup
        {
            get => _checkForNewVerionOnStartup;
            set => SetProperty(ref _checkForNewVerionOnStartup, value);
        }

        public bool IncludePrereleases
        {
            get => _includePrereleases;
            set => SetProperty(ref _includePrereleases, value);
        }

        public bool IsConnected
        {
            get => _IsConnected;
            set => SetProperty(ref _IsConnected, value);
        }

        public ListType ListType
        {
            get => _listType;
            set => SetProperty(ref _listType, value);
        }

        public string ReadFileAllowedActions
        {
            get => _allowedActions;
            set => SetProperty(ref _allowedActions, value);
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

        public string FilterRegex
        {
            get => filterRegex;
            set => SetProperty(ref filterRegex, value);
        }

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

        public bool SettingShowResult
        {
            get => _settingShowResults;
            set
            {
                SetProperty(ref _settingShowResults, value);
                if (!value)
                {
                    RemoveAllChannelsFromGrid();
                }
            }
        }


        public DataGrid DataGrid { get; set; }
        public bool RegexOptionIgnoreCase { get; set; }
        public bool RegexOptionMultiline { get; set; }
        public bool RegexOptionEcmaScript { get; set; }
        public bool RegexOptionCultureInvariant { get; set; }
        public List<string> SpecialUsers { get; set; }
        public bool ProtectedUserMode_Skip { get; set; }
        public string WindowTitle { get; set; }
        public bool ProtectedUserMode_Cancel { get; set; } = true;

        #region Commands

        private ReactiveCommand<Window, Unit> OpenFileCommand { get; }
        private ReactiveCommand<Window, Unit> OpenFileFromURLCommand { get; }
        private ReactiveCommand<Window, Unit> FetchLastFollowersForChannelCommand { get; }
        private ReactiveCommand<Window, Unit> SaveListAsCommand { get; }
        private ReactiveCommand<Window, Unit> SaveBanListAsCommand { get; }
        private ReactiveCommand<Window, Unit> SaveUnBanListAsCommand { get; }
        private ReactiveCommand<Window, Unit> CheckForNewVersionsCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearResultsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCredentialsCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveDataCommand { get; }
        public ReactiveCommand<Unit, Unit> StoreCredentialsCommand { get; }
        public ReactiveCommand<Window, Unit> OnClickPropertiesAddEntry { get; }
        public ReactiveCommand<Window, Unit> OnClickPropertiesPasteClipboard { get; }
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
        public ReactiveCommand<Window, Unit> ShowInfoWindowCommand { get; }
        public ReactiveCommand<Window, Unit> EditLastVisitChannelCommand { get; }
        public ReactiveCommand<Window, Unit> GetOAuthCommand { get; }
        public ReactiveCommand<MainWindow, Unit> TokenInfoCommand { get; }
        public ReactiveCommand<Unit, Task> QueryUsersInListCommand { get; }
        public ReactiveCommand<Unit, Unit> EditSpecialUsersCommand { get; }
        public ReactiveCommand<Unit, Unit> FetchModsVipsCommand { get; }

        #endregion


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

        #endregion

        #region Private Methods

        private void EditSpecialUsers()
        {
            if (!File.Exists(SpecialUserFilePath))
            {
                File.Create(SpecialUserFilePath).Close();

                File.WriteAllText(SpecialUserFilePath, "// example" + Environment.NewLine +
                                                       "// CommanderRoot" + Environment.NewLine +
                                                       "// teischEnte" + Environment.NewLine);
            }

            new Process
            {
                StartInfo = new ProcessStartInfo(SpecialUserFilePath)
                {
                    UseShellExecute = true
                }
            }.Start();
        }

        private void ClearResults()
        {
            IsBusy = true;
            Entries.AsParallel()
                .ForEach(x =>
                {
                    if (!x.IsValid)
                        return;
                    x.Result.Keys.ForEach(y => { x.Result[y] = string.Empty; });
                });
            IsBusy = false;
        }

        private async void SaveListAs(Window window, WorkingMode mode)
        {
            IsBusy = true;
            string title = string.Empty;
            if (mode == WorkingMode.Readfile)
            {
                title = "Save list as ...";
            }
            else if (mode == WorkingMode.Ban)
            {
                title = "Save Ban list as ...";
            }
            else if (mode == WorkingMode.Unban)
            {
                title = "Save Unban list as ...";
            }

            var filepath = await new SaveFileDialog()
            {
                DefaultExtension = "txt",
                Title = title,
                InitialFileName = "MassBanToolListExport",
                Filters = new List<FileDialogFilter>()
                {
                    new()
                    {
                        Extensions = new List<string>() { "txt" },
                        Name = "Text files"
                    },
                    new()
                    {
                        Extensions = new List<string>() { "*" },
                        Name = "All files"
                    },
                }
            }.ShowAsync(window);

            if (filepath == null)
            {
                return;
            }

            string result = string.Empty;
            if (mode == WorkingMode.Readfile)
            {
                result = string.Join(Environment.NewLine,
                    Entries.AsParallel().WithDegreeOfParallelism(8).Select(x => x.ChatCommand));
            }
            else if (mode == WorkingMode.Ban)
            {
                result = string.Join(Environment.NewLine,
                    Entries.AsParallel().WithDegreeOfParallelism(8).Select(x => $".ban {x.Name} {Reason}".Trim()));
            }
            else if (mode == WorkingMode.Unban)
            {
                result = string.Join(Environment.NewLine,
                    Entries.AsParallel().WithDegreeOfParallelism(8).Select(x => $".unban {x.Name}".Trim()));
            }

            await File.WriteAllTextAsync(filepath, result);
            IsBusy = false;
            GC.Collect();
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
                _lastVisitedChannels = inputVM.Objects.Select(x => x.Value.ToLower()).ToList();
                BuildLastVisitChannelContextMenu();
            }
        }

        private void StoreCredentials()
        {
            if (string.IsNullOrEmpty(OAuth))
                return;
            SecretHelper.StoreCredentials(OAuth);
        }


        private void ApplySettings()
        {
            var data = Program.settings;

            if (data != null && data.RequestsPerMinute != default) MessageDelay = data.RequestsPerMinute.ToString();

            if (data?.LastVisitedChannels != null)
            {
                _lastVisitedChannels = data.LastVisitedChannels.ToList();
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

            if (data != null)
                _settingLoadCredentialsOnStartup = data.LoadCredentialOnStartup;

            LogViewModel.Log("Done loading setting for this User.");

            if (_settingLoadCredentialsOnStartup)
            {
                LoadCredentials();
            }

            if (data != null)
            {
                _checkForNewVerionOnStartup = data.CheckForUpdates;
                _includePrereleases = data.IncludePrereleases;
            }

            if (_checkForNewVerionOnStartup)
            {
                CheckForNewVersions((App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                    ?.MainWindow);
            }
        }

        private void BuildLastVisitChannelContextMenu()
        {
            _lastVisitedChannelsMenu = new ContextMenu();

            MenuItem item;
            _lastVisitedChannelsMenu.Items.Clear();
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
                _lastVisitedChannelsMenu.Items.Add(item);
            }

            RaisePropertyChanged(nameof(LastVisitedChannelsMenu));
        }

        private void SaveData()
        {
            var fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MassBanTool");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var data = new SettingsWrapper()
            {
                LastVisitedChannels = new HashSet<string>(_lastVisitedChannels),
                AllowedActions = _allowedActions.Split(Environment.NewLine)
                    .Select(x => x.Trim())
                    .Where(x => x != string.Empty)
                    .ToHashSet(),
                RequestsPerMinute = _messageDelay,
                LoadCredentialOnStartup = SettingLoadCredentialsOnStartup,
                CheckForUpdates = CheckForNewVerionOnStartup,
                IncludePrereleases = IncludePrereleases,
                Version = Program.Version
            };

            var result = data.ToJSON();

            File.WriteAllText(fileName, result);
        }

        #endregion

        #region Constants

        private const string MESSAGE_DELAY_TOO_LOW = "Delay may not be under 0ms";
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
        private const string API_CACTUSTOOL_LASTFOLLOWERS = "https://cactus.tools/twitch/followers";

        private const string QUESTION_LISTTYPEMISMATCHRUN =
            "Listtype does not match the selected operation mode. Do you want to ignore the additional params and run this anyways?";

        private const string LSITTYPEMISMATCH = "Listtype Mismatch";
        private const string ResultDryBanned = "Dry Banned";
        private const string ResultBanned = "Banned";
        private const string ResultAlreadyBanned = "Already Banned";
        private const string InvalidBanTarget = "Cannot be Banned";
        private const string NotBanned = "UserNotBanned";

        #endregion

        #region Fields

        private string _allowedActions;

        private double _banProgress;
        private string _channelS = string.Empty;
        private bool _checkForNewVerionOnStartup;
        private bool _dryRun;

        private ObservableCollection<Entry> _entries;
        private TimeSpan _eta;
        private bool _includePrereleases;
        private bool _IsConnected;
        private List<string> _lastVisitedChannels = new();
        private ContextMenu? _lastVisitedChannelsMenu;
        private bool _listFilterRemoveMatching = false;
        private ListType _listType;
        private LogWindow _logWindow;
        private int _messageDelay = 300;
        private string _oAuth;

        private bool _paused;
        private bool _protectSpecialUsers = true;
        private bool _readFileCommandMismatchCancel = true;
        private bool _readFileCommandMismatchSkip;
        private bool _settingShowResults = true;
        private string _reason;
        private bool _settingLoadCredentialsOnStartup;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;
        private string _toolStatus;
        private string _userId;
        private Mutex _userMutex;
        private string _username;

        private Task _worker;
        // Key is username, value is ID
        private Dictionary<string, string> channelIDs = new Dictionary<string, string>();
        private List<string> channels = new();
        private string filterRegex = string.Empty;

        private string SpecialUserFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MassBanTool",
                "MassBanToolProtectedUsers.txt");

        private List<string> TokenScopes = new List<string>();
        #endregion
    }
}