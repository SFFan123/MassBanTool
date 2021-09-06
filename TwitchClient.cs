using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace MassBanTool
{
    public class TwitchChatClient
    {
        public static bool mt_pause = false;

        private int ActionListLenght = 0;

        TwitchClient client;

        private ConnectionCredentials credentials;

        private Form form;

        Task messageTask = null;

        private bool reconnect = true;

        private bool running = true;

        public TwitchChatClient(string user, string oauth, string channel, Form f)
        {
            form = f;
            this.user = user;
            this.oauth = oauth;
            credentials = new ConnectionCredentials(user, oauth);

            this.channel = channel;

            client = InitializeClient();

            RegisterClientEventHandler(client);

            client.Connect();
        }

        public string channel { get; set; }

        public int cooldown { get; set; }

        public string oauth { get; set; }

        public string user { get; set; }

        public List<string> MessagesQueue { get; private set; } = new List<string>();

        public bool isMod { get; private set; }

        public bool isBroadcaster { get; private set; }

        private void RegisterClientEventHandler(TwitchClient client)
        {
            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnConnected += Client_OnConnected;
            client.OnUserStateChanged += Client_OnUserStateChanged;
            client.OnDisconnected += Client_OnDisconnected;
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            form.setInfo(this, e.Channel);
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
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                running = false;
                client.Disconnect();
            };
            return client;
        }


        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            if (!reconnect)
                return;

            Console.WriteLine("Connection to Twitch lost");

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

            form.setMod(this, isMod, isBroadcaster);

            if (isBroadcaster)
            {
                form.ShowWarning(
                    "You are the Broadcaster, this mean you could also ban your mods/bots by accident! Make sure they aren't in the list you going to ban!");
            }

            if (!isMod && !isBroadcaster)
            {
                form.ThrowError("Neither Broadcaster nor Mod in the given channel!, Exiting.");
            }
        }


        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
            messageTask = makeMessageSender();
            messageTask.Start();
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
                        while (mt_pause)
                        {
                            Task.Delay(1000);
                        }

                        if (MessagesQueue.Count > 0)
                        {
                            message = MessagesQueue[0];
                            MessagesQueue.RemoveAt(0);
                            int banindex = ActionListLenght - MessagesQueue.Count;
                            if (banindex % 10 == 0)
                            {
                                Console.WriteLine(banindex);
                                eta = TimeSpan.FromMilliseconds((MessagesQueue.Count * cooldown));
                                form.setBanProgress(this, banindex, ActionListLenght);
                                form.setETA(this, eta.ToString("g"));
                            }

                            client.SendMessage(channel, message);
                            if (MessagesQueue.Count == 0)
                            {
                                form.setBanProgress(this, 100, 100);
                                form.setETA(this, "-");
                            }

                            Console.WriteLine($"MT: {DateTime.Now.ToString("dd.MM H:mm:ss")} > {message}");

                            Thread.Sleep(cooldown);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{e.GetType().Name} {e.Message}", "ERROR");
                    Environment.Exit(-1);
                }
            }, TaskCreationOptions.LongRunning);
        }


        public void switchChannel(string newChannel)
        {
            newChannel = newChannel.Trim();
            if (newChannel.Equals(string.Empty))
            {
                throw new ArgumentException("channel may not be empty");
            }

            client.LeaveChannel(channel);
            channel = newChannel;
            client.JoinChannel(channel);
        }

        public void setToBann(List<string> toBan, string reason)
        {
            MessagesQueue.Clear();
            ActionListLenght = toBan.Count;
            if (reason.Trim().Equals(String.Empty))
            {
                reason = "no reason given.";
            }

            for (int i = 0; i < toBan.Count; i++)
            {
                MessagesQueue.Add(($"/ban {toBan[i].Trim()} {reason.Trim()}").Trim());
            }
        }


        public void Abort()
        {
            mt_pause = true;
            MessagesQueue.Clear();
            mt_pause = false;
        }

        public void setToUNBann(List<string> toUnBan)
        {
            MessagesQueue.Clear();
            ActionListLenght = toUnBan.Count;
            for (int i = 0; i < toUnBan.Count; i++)
            {
                MessagesQueue.Add(($"/unban {toUnBan[i].Trim()}").Trim());
            }
        }

        public void addRawMessages(List<string> commandList)
        {
            MessagesQueue.Clear();
            ActionListLenght = commandList.Count;

            MessagesQueue = commandList;
        }
    }
}