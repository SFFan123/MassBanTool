using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using IX.Observable;
using MassBanToolMP.Helper;
using MassBanToolMP.Models;
using MassBanToolMP.Views;
using MassBanToolMP.Views.Dialogs;
using MessageBox.Avalonia.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Chat.ChatSettings;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace MassBanToolMP.ViewModels
{
    public partial class MainWindowViewModel
    {
        private void GetTokenRateLimit(out int rateLimit, out TimeSpan resetSpan, out DateTime resultTime)
        {
            rateLimit = -1;
            resetSpan = TimeSpan.Zero;
            resultTime = default;
            
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/streams?first=1");
                request.Headers.Add("Authorization", Program.API.Settings.AccessToken);
                request.Headers.Add("Client-Id", Program.API.Settings.ClientId);
                var response = httpClient.Send(request);

                if (!response.IsSuccessStatusCode)
                {
                    // TODO
                    return;
                }
                
                if( response.Headers.TryGetValues("Ratelimit-Limit", out var limitValues) )
                {
                    string val = limitValues?.FirstOrDefault() ?? string.Empty;
                    if (!string.IsNullOrEmpty(val))
                    {
                        rateLimit = int.Parse(val);
                    }
                }

                DateTime resetTime = default;
                resultTime = default;

                if (response.Headers.TryGetValues("Ratelimit-Reset", out var resetValues))
                {
                    string val = resetValues?.FirstOrDefault() ?? string.Empty;
                    if (!string.IsNullOrEmpty(val))
                    {
                        resetTime = DateTime.UnixEpoch.AddSeconds(int.Parse(val)).ToLocalTime();
                    }
                }

                if (response.Headers.TryGetValues("Date", out var dateValues))
                {
                    resultTime = DateTime.Parse(dateValues.First());
                }

                resetSpan = resetTime - resultTime;
            }
        }


        private async Task BanUser(string channelId, string channelName, Entry entry, string reason, int? duration = null)
        {
            bool breakLoop = false;
            BanUserResponse? banRespone = null;
            BanUserRequest request = new BanUserRequest
            {
                Reason = reason,
                UserId = entry.Id
            };
            if (duration.HasValue)
            {
                request.Duration = duration.Value;
            }

            for (ushort tries = 0; tries < 5 && !breakLoop; tries++)
            {
                try
                {
                    banRespone = await Program.API.Helix.Moderation.BanUserAsync(channelId, _userId, request);
                }
                catch (BadRequestException e)
                {
                    var responseText = await e.HttpResponse.Content.ReadAsStringAsync();
                    var job = JsonNode.Parse(responseText);
                    string res = job["message"].ToString();

                    switch (res)
                    {
                        case "The user specified in the user_id field is already banned.":
                            OnUserAlreadyBanned(channelName, entry.Name);
                            return;

                        case "The user specified in the user_id field may not be banned.":
                            OnInvalidBanTarget(channelName, entry.Name);
                            return;

                        default:
                            throw;
                    }
                }
                catch (TooManyRequestsException)
                {
                    LogViewModel.Log("Hitting Rate Limit");
                    IncreaseDelay();
                }

                if (banRespone?.Data != null)
                {
                    OnUserBanned(channelName, entry.Name);
                    breakLoop = true;
                }
            }
        }

        private async Task UnBanUser(string channelId, string channelName, Entry entry)
        {
            bool breakLoop = false;
            for (ushort tries = 0; !breakLoop && tries < 5; tries++)
            {
                try
                {
                    await Program.API.Helix.Moderation.UnbanUserAsync(channelId, _userId, entry.Id);
                }
                catch (BadRequestException e)
                {
                    var responseText = await e.HttpResponse.Content.ReadAsStringAsync();
                    var job = JsonNode.Parse(responseText);
                    string res = job["message"].ToString();

                    switch (res)
                    {
                        case "The user specified in the user_id query parameter is not banned.":
                            LogViewModel.Log($"The user '{entry.Name}' is not banned in '{channelName}'.");
                            return;

                        default:
                            throw;
                    }
                }
                catch (TooManyRequestsException)
                {
                    LogViewModel.Log("Hitting Rate Limit");
                    IncreaseDelay();
                }
            }
        }

        private async Task QueryIdsForEntries()
        {
            bool busyState = IsBusy;
            IsBusy = true;

            var toFetch = Entries.AsParallel().Where(x => string.IsNullOrEmpty(x.Id)).ToList();

            if (!toFetch.Any())
            {
                IsBusy = busyState;
                return;
            }

            int resCount = 0;
            try
            {
                LogViewModel.Log($"Fetching IDs for {toFetch.Count} entries...");

                for (int i = 0; i < toFetch.Count; i += 100)
                {
                    if (Paused)
                    {
                        await Task.Delay(50);
                    }
                    if (_token.IsCancellationRequested)
                        return;

                    ToolStatus = $"Fetching IDs for list entries... ({i}/{toFetch.Count})";

                    var entriesSlice = toFetch.Skip(i).Take(100);
                    var logins = entriesSlice.Select(x => x.Name).ToList();
                    var res = await Program.API.Helix.Users.GetUsersAsync(logins: logins);

                    resCount += res.Users.Length;

                    foreach (User resUser in res.Users)
                    {
                        var u = entriesSlice.FirstOrDefault(x =>
                            string.Equals(x.Name, resUser.Login, StringComparison.InvariantCultureIgnoreCase));
                        if (u != null)
                        {
                            u.Id = resUser.Id;
                        }
                    }
                }

                LogViewModel.Log(
                    $"Fetched IDs. Out of {toFetch.Count} usernames {resCount} were found by twitch, entries which weren't found will be skipped - Accounts probably already banned by twitch.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogViewModel.Log("Error while Fetching Ids for entries. " + e.Message);
            }

            if (toFetch.Count != resCount)
            {
                var diagres =
                    await MessageBox.Show(
                        $"Only {resCount} out of {toFetch.Count} were found on Twitch, do you want to remove the invalid/banned entries? These will be skipped anyways.",
                        "Invalid users", ButtonEnum.YesNo);

                if (diagres == ButtonResult.Yes)
                {
                    var list = Entries.Where(x => string.IsNullOrEmpty(x.Id)).ToList();
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Entries.RemoveMany(list);
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => Entries.RemoveMany(list));
                    }
                    
                }
            }

            ToolStatus = string.Empty;
            IsBusy = false;
        }

        private async Task GetChannelIds()
        {
            bool busystate = IsBusy;
            IsBusy = true;

            #region SanityChecks

            if (channels.Count == 0)
            {
                return;
            }

            if (Program.API.Settings.AccessToken == null)
            {
                return;
            }

            #endregion

            ToolStatus = "Fetching IDs for channels...";

            Stopwatch sw = new Stopwatch();

            var userId = await Program.API.Helix.Users.GetUsersAsync(logins: channels);

            if (userId != null)
            {
                foreach (User user in userId.Users)
                {
                    channelIDs[user.Login] = user.Id;
                }
            }

            ToolStatus = string.Empty;
            IsBusy = busystate;
        }

        private async void ExecReadFileEntry(string channelID, string channelName, Entry entry)
        {
            ReadFileOperation operation;
            var api = Program.API.Helix;
            try
            {
                operation = parseOperation(entry.Command);
            }
            catch (Exception exception)
            {
                LogViewModel.Log(exception.Message);
                return;
            }

            switch (operation)
            {
                case ReadFileOperation.AddBlockedTerm:
                {
                    try
                    {
                        await api.Moderation.AddBlockedTermAsync(channelID, _userId, entry.Name + entry.Reason);
                    }
                    catch (BadRequestException)
                    {
                        LogViewModel.Log(
                            "1. The text field is required.  2. The length of the term in the text field is either too short or too long.");
                    }
                    catch (Exception e)
                    {
                        LogViewModel.Log(e.Message);
                    }

                    break;
                }
                case ReadFileOperation.RemoveBlockedTerm:
                {
                    try
                    {
                        await api.Moderation.DeleteBlockedTermAsync(channelID, _userId, entry.Name + entry.Reason);
                    }
                    catch (BadRequestException)
                    {
                        LogViewModel.Log(
                            "1. The id query parameter is required");
                    }
                    catch (Exception e)
                    {
                        LogViewModel.Log(e.Message);
                    }

                    break;
                }
                case ReadFileOperation.Ban:
                {
                    await BanUser(channelID, channelName, entry, entry.Reason);

                    break;
                }
                case ReadFileOperation.UnBan:
                case ReadFileOperation.UnTimeout:
                {
                    try
                    {
                        await api.Moderation.UnbanUserAsync(channelID, _userId, entry.Id);
                    }
                    catch (BadRequestException)
                    {
                        LogViewModel.Log("User not banned");
                    }
                    catch (Exception e)
                    {
                        LogViewModel.Log("ERROR: Unexpected Exception: " + e.Message);
                    }

                    break;
                }

                case ReadFileOperation.Block:
                {
                    await api.Users.BlockUserAsync(entry.Id);
                    break;
                }
                case ReadFileOperation.UnBlock:
                {
                    await api.Users.UnblockUserAsync(entry.Id);
                    break;
                }
                case ReadFileOperation.Vip:
                {
                    await api.Channels.AddChannelVIPAsync(channelID, entry.Id);
                    break;
                }
                case ReadFileOperation.UnVip:
                {
                    await api.Channels.RemoveChannelVIPAsync(channelID, entry.Id);
                    break;
                }
                case ReadFileOperation.Mod:
                {
                    await api.Moderation.AddChannelModeratorAsync(channelID, entry.Id);
                    break;
                }
                case ReadFileOperation.UnMod:
                {
                    await api.Moderation.DeleteChannelModeratorAsync(channelID, entry.Id);
                    break;
                }
                case ReadFileOperation.Timeout:
                {
                    int duration = 600;
                    string reason = entry.Reason;
                    if (string.IsNullOrEmpty(entry.Reason))
                    {
                        int idx = entry.Reason.IndexOf(" ");
                        if (idx >= 0)
                        {
                            string s_duration = entry.Reason.Substring(0, idx);
                            duration = int.Parse(s_duration);
                            reason = reason.Substring(idx + 1);
                        }
                    }

                    await api.Moderation.BanUserAsync(channelID, _userId, new BanUserRequest()
                    {
                        Duration = duration,
                        Reason = reason,
                        UserId = entry.Id
                    });
                    break;
                }

                case ReadFileOperation.Clear:
                {
                    await api.Moderation.DeleteChatMessagesAsync(channelID, _userId);
                    break;
                }
                case ReadFileOperation.Slow:
                {
                    int duration = 30;
                    try
                    {
                        duration = int.Parse(entry.Reason.Trim());
                    }
                    catch
                    {
                        // ignored
                    }

                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        SlowMode = true,
                        SlowModeWaitTime = duration
                    });
                    break;
                }
                case ReadFileOperation.SlowOff:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        SlowMode = false
                    });
                    break;
                }
                case ReadFileOperation.Followers:
                {
                    int? duration = null;
                    try
                    {
                        duration = int.Parse(entry.Reason.Trim());
                    }
                    catch
                    {
                        // ignored
                    }

                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        FollowerMode = true,
                        FollowerModeDuration = duration
                    });
                    break;
                }
                case ReadFileOperation.FollowersOff:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        FollowerMode = false
                    });
                    break;
                }
                case ReadFileOperation.Subscribers:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        SubscriberMode = true
                    });
                    break;
                }
                case ReadFileOperation.SubscribersOff:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        SubscriberMode = false
                    });
                    break;
                }
                case ReadFileOperation.Uniquechat:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        UniqueChatMode = true
                    });
                    break;
                }
                case ReadFileOperation.UniquechatOff:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        UniqueChatMode = false
                    });
                    break;
                }
                case ReadFileOperation.Emoteonly:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        EmoteMode = true
                    });
                    break;
                }
                case ReadFileOperation.EmoteonlyOff:
                {
                    await api.Chat.UpdateChatSettingsAsync(channelID, _userId, new ChatSettings()
                    {
                        EmoteMode = false
                    });
                    break;
                }
                default:
                {
                    LogViewModel.Log("I have no memory of this place.");
                    Console.WriteLine("I have no memory of this place.");
                    break;
                }
            }
        }

        private async void GetAccessToken(Window window)
        {
            var diag = new GetLoginFlow();
            await diag.ShowDialog(window);

            if (diag.Result != DialogResult.OK)
                return;
            _username = diag.Username;
            Program.API.Settings.AccessToken = "Bearer " + diag.Token;
            OAuth = Program.API.Settings.AccessToken;
            StoreCredentials();
        }
        
        private Task CreateWorkerTask(WorkingMode mode)
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Paused = false;

            int maxParallelism = 8;
            
            return Task.Factory.StartNew(() =>
            {
                List<Task> Tasks = new List<Task>();

                QueryIdsForEntries().Wait();
                if (_token.IsCancellationRequested)
                    return;

                if (Entries.Count == 0)
                    return;

                if (channelIDs.All(x => string.IsNullOrEmpty(x.Value)))
                    return;

                foreach (Entry entry in _entries)
                foreach (string channel in channels)
                {
                    if (!entry.Result.ContainsKey(channel) || entry.Result[channel] == null)
                        entry.Result[channel] = string.Empty;
                    entry.Result = new ConcurrentObservableDictionary<string, string>(entry.Result);
                }

                var textReason = Reason ?? string.Empty;
                try
                {
                    for (var i = 0; i < Entries.Count; i++)
                    {
                        while (Paused)
                        {
                            Task.Delay(1000, _token).Wait();
                        }

                        while (Tasks.Count >= maxParallelism)
                        {
                            Task.Delay(20, _token).Wait();
                        }
                        
                        if (_token.IsCancellationRequested) 
                            break;

                        Entry entry = Entries[i];
                        Task tsk = Task.CompletedTask;
                        foreach (var channel in channelIDs)
                        {
                            if (!entry.IsValid) break;

                            if (entry.Result.ContainsKey(channel.Key.ToLower()) &&
                                !string.IsNullOrEmpty(entry.Result[channel.Key.ToLower()])) continue;

                            switch (mode)
                            {
                                case WorkingMode.Ban:
                                {
                                    tsk = BanUser(channel.Value, channel.Key, entry, textReason);

                                    Tasks.Add(tsk);

                                    tsk.ContinueWith((t) => { Tasks.Remove(t); });

                                    break;
                                }
                                case WorkingMode.Unban:
                                {
                                    tsk = UnBanUser(channel.Value, channel.Key, entry);

                                    Tasks.Add(tsk);

                                    tsk.ContinueWith((t) => { Tasks.Remove(t); });

                                    break;
                                }
                                case WorkingMode.Readfile:
                                {
                                    ExecReadFileEntry(channel.Value, channel.Key, entry);
                                    break;
                                }
                            }

                            Task.Delay(TimeSpan.FromMilliseconds(_messageDelay), _token).Wait();
                        }

                        if (i % 2 == 0)
                        {
                            BanProgress = (i + 1) / (double)Entries.Count * 100;
                            ETA = TimeSpan.FromMilliseconds((Entries.Count - i + 1) * channels.Count * (_messageDelay) +
                                                            (Entries.Count - i + 1) * 100);
                        }
                    }

                    if (!_token.IsCancellationRequested)
                    {
                        BanProgress = 100;
                    }
                }
                catch (Exception e)
                {
                    if (!_token.IsCancellationRequested)
                        ToolStatus = "Error!";

                    LogViewModel.Log(e.GetType().Name + e.Message);
                }

                ETA = TimeSpan.Zero;
            }, _token, TaskCreationOptions.LongRunning|TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }
        
        private void IncreaseDelay()
        {
            _messageDelay += 3;
            RaisePropertyChanged(nameof(MessageDelay));
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }

        private async void ShowTokenInfo(MainWindow arg)
        {
            IsBusy = true;
            var res = await Program.API.Auth.ValidateAccessTokenAsync(Program.API.Settings.AccessToken);
            GetTokenRateLimit(out var limit, out var span, out var resultTime);
            double rate = limit / span.TotalSeconds; 
            IsBusy = false;
            await new TokenInfoDialog(res, resultTime, rate).ShowDialog(arg);
        }
    }
}