using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace MassBanTool
{
    public class TwitchChatClient
    {
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

        private Form form;

        public string oauth { get; set; }

        public string user { get; set; }

        public static bool mt_pause = false;
        Task messageThread = null;

        private int toBanLenght = 0;

        public LinkedList<string> MessagesQueue { get; private set; } = new LinkedList<string>();

        public bool isMod
        {
            get;
            private set;
        }
        public bool isBroadcaster
        {
            get;
            private set;
        }

        TwitchClient client;

        private ConnectionCredentials credentials;
        private void RegisterClientEventHandler(TwitchClient client)
        {
            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            //client.OnMessageReceived += Client_OnMessageReceived;
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
            return client;
        }

        
        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
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
            isBroadcaster = string.Equals(e.UserState.Channel, e.UserState.DisplayName, StringComparison.CurrentCultureIgnoreCase);

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
            messageThread = makeMessageSender();
            messageThread.Start();
        }



        private Task makeMessageSender()
        {
            return new Task(() =>
            {
                try
                {
                    TimeSpan eta;
                    Thread.CurrentThread.IsBackground = true;
                    Thread.CurrentThread.Name = "MessageSenderThread";
                    string message;
                    while (true)
                    {
                        while (mt_pause)
                        {
                            Task.Delay(1000);
                        }

                        if (MessagesQueue.Count > 0)
                        {
                            message = MessagesQueue.First.Value;
                            MessagesQueue.RemoveFirst();
                            int banindex = (toBanLenght - MessagesQueue.Count);
                            if (banindex % 10 == 0)
                            {
                                Console.WriteLine(banindex);
                                eta = TimeSpan.FromMilliseconds((MessagesQueue.Count * cooldown));
                                form.setBanProgress(this, banindex, toBanLenght);
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

        public void addToBann(List<string> toBan, string reason)
        {
            toBanLenght = toBan.Count;
            if (reason.Trim().Equals(String.Empty))
            {
                reason = "no reason Given.";
            }

            for (int i = 0; i < toBan.Count; i++)
            {
                MessagesQueue.AddLast(($"/ban {toBan[i].Trim()} {reason.Trim()}").Trim());
            }
        }


        public void Abort()
        {
            mt_pause = true;
            MessagesQueue.Clear();
            mt_pause = false;
        }
    }
}
