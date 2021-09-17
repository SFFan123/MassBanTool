using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace MassBanTool
{
    public class TwitchChatClient : INotifyPropertyChanged
    {
        private static bool _mtpause = false;

        private MainWindow _mainWindow;

        private bool abort = false;

        private int ActionListLenght = 0;

        TwitchClient client;

        private ConnectionCredentials credentials;

        private Regex ircBanConfirmrRegex = new Regex(@"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        Task messageTask = null;


        private bool running = true;

        public TwitchChatClient(string user, string oauth, List<string> channel, MainWindow f)
        {
            _mainWindow = f;
            this.user = user;
            this.oauth = oauth;
            credentials = new ConnectionCredentials(user, oauth);


            this.channel = channel;

            client = InitializeClient();

            RegisterClientEventHandler(client);

            client.Connect();
        }

        public static bool mt_pause
        {
            get => _mtpause;
            set
            {
                log($"Pause flag set to {value}", true);
                _mtpause = value;
            }
        }

        public ToolStatus CurrentStatus { get; set; } = ToolStatus.Disconnected;
        public ToolStatus TargetStatus_for_pause { get; set; }

        public List<string> channel { get; set; }

        public int cooldown { get; set; } = 500;

        public string oauth { get; set; }

        public string user { get; set; }

        public List<string> MessagesQueue { get; private set; } = new List<string>();

        public bool isMod { get; private set; }

        public bool isBroadcaster { get; private set; }

        public Dictionary<string, List<string>> ChannelModerators { get; private set; } =
            new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> ChannelVIPs { get; private set; } =
            new Dictionary<string, List<string>>();

        public event PropertyChangedEventHandler PropertyChanged;


        private void RegisterClientEventHandler(TwitchClient client)
        {
            client.OnLog += Client_OnLog;
            client.OnIncorrectLogin += ClientOnOnIncorrectLogin;
            client.OnUserBanned += Client_OnUserBanned;
            client.OnConnected += Client_OnConnected;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnUserStateChanged += Client_OnUserStateChanged;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnModeratorsReceived += ClientOnOnModeratorsReceived;
            client.OnVIPsReceived += ClientOnOnVIPsReceived;
        }

        private void ClientOnOnVIPsReceived(object sender, OnVIPsReceivedArgs e)
        {
            ChannelVIPs[e.Channel.ToLowerInvariant()] = e.VIPs;
        }

        private void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            //Console.WriteLine(e.UserBan.Username);
        }

        private void ClientOnOnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            _mainWindow.ThrowError($"Invalid Credentials.", false);
        }

        private void ClientOnOnModeratorsReceived(object sender, OnModeratorsReceivedArgs e)
        {
            ChannelModerators[e.Channel.ToLowerInvariant()] = e.Moderators;
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            _mainWindow.setInfo(this, e.Channel);
            CurrentStatus = ToolStatus.Ready;
            NotifyPropertyChanged(nameof(CurrentStatus));
        }

        private TwitchClient InitializeClient()
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 100,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                UseSsl = true,
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            TwitchClient client;
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);
            return client;
        }


        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            TwitchChatClient.mt_pause = true;
            messageTask.Dispose();
            CurrentStatus = ToolStatus.Disconnected;
            NotifyPropertyChanged(nameof(CurrentStatus));

            Console.WriteLine("Connection to Twitch lost");
            log("Connection to Twitch lost");


            client = null; // Throw the client into the trashcan

            Thread.Sleep(2000);


            client = InitializeClient(); // make a new one.

            RegisterClientEventHandler(client); // register the client events.

            client.Connect(); // rejoin.
        }

        private void Client_OnUserStateChanged(object sender, OnUserStateChangedArgs e)
        {
            isMod = e.UserState.IsModerator;
            isBroadcaster = string.Equals(e.UserState.Channel, e.UserState.DisplayName,
                StringComparison.CurrentCultureIgnoreCase);

            _mainWindow.setMod(this, isMod, isBroadcaster);

            client.GetChannelModerators(e.UserState.Channel);
            client.GetVIPs(e.UserState.Channel);

            if (!isMod && !isBroadcaster)
            {
                _mainWindow.ThrowError("Neither Broadcaster nor Mod in the given channel!, Exiting.", true, true);
            }
        }


        private void Client_OnLog(object sender, OnLogArgs e)
        {
            string to_log = $"{e.DateTime}: {e.BotUsername} - {e.Data}";
            log(to_log);
            Console.WriteLine(to_log);
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            messageTask = makeMessageSender();
            messageTask.Start();
            mt_pause = false;
        }


        private Task makeMessageSender()
        {
            return new Task(() =>
            {
                try
                {
                    TimeSpan eta;
                    string message;
                    while (running)
                    {
                        if (abort)
                        {
                            MessagesQueue.Clear();
                            log("Queue cleared", true);
                            abort = false;
                        }

                        while (mt_pause)
                        {
                            Thread.Sleep(1000);
                        }

                        if (MessagesQueue.Count > 0)
                        {
                            message = MessagesQueue[0];

                            int banindex = ActionListLenght - MessagesQueue.Count;
                            if (banindex % 10 == 0)
                            {
                                eta = TimeSpan.FromMilliseconds((MessagesQueue.Count * cooldown));
                                _mainWindow.setBanProgress(this, banindex, ActionListLenght);
                                _mainWindow.setETA(this, eta.ToString("g"));
                            }

                            foreach (var channel in client.JoinedChannels)
                            {
                                client.SendMessage(channel, message);

                                string to_log = $"MT: {DateTime.Now:HH:mm:ss fff} >#{channel.Channel} - {message}";
                                Console.WriteLine(to_log);
                                log(to_log);

                                Thread.Sleep(cooldown);
                            }


                            MessagesQueue.RemoveAt(0);

                            if (MessagesQueue.Count == 0)
                            {
                                _mainWindow.setBanProgress(this, 100, 100);
                                _mainWindow.setETA(this, "-");
                            }

                            //Thread.Sleep(cooldown);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    cooldown += 10;
                    _mainWindow.increaseDelay(cooldown);
                    Thread.Sleep(10);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    if (!mt_pause)
                    {
                        _mainWindow.ThrowError($"{e.GetType().Name} {e.Message} \n{e.StackTrace}", false);
                    }
                }
                catch (Exception e)
                {
                    _mainWindow.ThrowError($"{e.GetType().Name} {e.Message} \n{e.StackTrace}", false);
                }
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);
        }


        public void switchChannel(List<string> newChannel)
        {
            var joinedChannels = client.JoinedChannels.Select(x => x.Channel.ToLower());

            var toleave = joinedChannels.Except(newChannel).ToList();

            var tojoin = newChannel.Except(joinedChannels).ToList();

            foreach (var channel in toleave)
            {
                client.LeaveChannel(channel);
                ChannelModerators[channel]?.Clear();
            }

            foreach (var channel in tojoin)
            {
                client.JoinChannel(channel, true);
            }


            CurrentStatus = ToolStatus.Ready;
            NotifyPropertyChanged(nameof(CurrentStatus));
        }

        public void setToBann(List<string> toBan, string reason)
        {
            MessagesQueue.Clear();
            ActionListLenght = toBan.Count;
            if (reason.Trim().Equals(String.Empty))
            {
                reason = "no reason given.";
            }

            CurrentStatus = ToolStatus.Banning;
            NotifyPropertyChanged(nameof(CurrentStatus));

            List<string> allSpecialChannelUser = new List<string>();

            foreach (var elm in ChannelModerators)
            {
                allSpecialChannelUser.AddRange(elm.Value);
            }

            foreach (var elm in ChannelVIPs)
            {
                allSpecialChannelUser.AddRange(elm.Value);
            }


            for (int i = 0; i < toBan.Count; i++)
            {
                if (allSpecialChannelUser.Contains(toBan[i].ToLower()))
                {
                    continue;
                }

                MessagesQueue.Add(($"/ban {toBan[i].Trim()} {reason.Trim()}"));
            }
        }


        public void Abort()
        {
            abort = true;
            CurrentStatus = ToolStatus.Aborted;
        }

        public void setToUNBann(List<string> toUnBan)
        {
            CurrentStatus = ToolStatus.UnBanning;
            NotifyPropertyChanged(nameof(CurrentStatus));
            MessagesQueue.Clear();
            ActionListLenght = toUnBan.Count;

            for (int i = 0; i < toUnBan.Count; i++)
            {
                MessagesQueue.Add(($"/unban {toUnBan[i].Trim()}").Trim());
            }
        }

        public void addRawMessages(List<string> commandList)
        {
            CurrentStatus = ToolStatus.ReadFile;
            NotifyPropertyChanged(nameof(CurrentStatus));
            MessagesQueue.Clear();
            ActionListLenght = commandList.Count;
            MessagesQueue = commandList;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private static void log(string line, bool addTimeStamp = false)
        {
            if (MainWindow.logwindow != null && !MainWindow.logwindow.IsDisposed)
            {
                if (addTimeStamp)
                {
                    line = DateTime.Now.ToString("G") + " " + line;
                }

                MainWindow.logwindow.Log(line);
            }
        }
    }
}