using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MassBanToolMP.Models;
using MassBanToolMP.ViewModels;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Exceptions;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace MassBanToolMP
{
    public class TwitchChatClient
    {
        public TwitchChatClient(MainWindowViewModel owner, string user, string oauth, List<string> channel)
        {
            this.owner = owner;
            credentials = new ConnectionCredentials(user, oauth);
            
            this.channel = channel;

            client = InitializeClient();

            RegisterClientEventHandler(client);

            client.Connect();
        }


        //Move to VM
        /*
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


            //CurrentStatus = ToolStatus.Ready;
            //NotifyPropertyChanged(nameof(CurrentStatus));
        }
        */

        private TwitchClient InitializeClient()
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 100,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                UseSsl = true
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);
            return client;
        }

        private void RegisterClientEventHandler(TwitchClient client)
        {
            client.OnLog += Client_OnLog;
            client.OnIncorrectLogin += Client_OnIncorrectLogin;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnUserStateChanged += Client_OnUserStateChanged;
            client.OnDisconnected += Client_OnDisconnected;
        }
        
        private void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            if (ManualDisconnect)
            {
                return;
            }
            client.Reconnect();
        }

        private void Client_OnUserStateChanged(object? sender, OnUserStateChangedArgs e)
        {
            if (e.UserState.IsModerator || e.UserState.Channel.ToLower() == client.TwitchUsername.ToLower())
            {
                return;
            }

            owner.MissingPermissions(e.UserState.Channel);
            client.Disconnect();
        }

        private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            client.GetChannelModerators(e.Channel);
            client.GetVIPs(e.Channel);
        }

        private void Client_OnIncorrectLogin(object? sender, OnIncorrectLoginArgs e)
        {
            client.Disconnect();
        }

        private void Client_OnLog(object? sender, OnLogArgs e)
        {
            string to_log = $"{e.DateTime}: {e.BotUsername} - {e.Data}";
            log(to_log);
            Trace.WriteLine(to_log);
        }


        private Task MakeMessageSenderTask()
        {
            return new Task(() =>
            {
                try
                {
                    TimeSpan eta;
                    string message;
                    int index = 0;
                    while (running)
                    {
                        if (abort)
                        {
                            entires = new ObservableCollection<Entry>();
                            log("Queue cleared", true);
                            abort = false;
                            index = 0;
                        }

                        while (paused)
                        {
                            Thread.Sleep(1000);
                        }

                        if (entires.Count > 0)
                        {
                            message = entires[index].ToString();

                            //int banindex = ActionListLenght - MessagesQueue.Count;
                            //if (banindex % 10 == 0)
                            //{
                            //    eta = TimeSpan.FromMilliseconds((MessagesQueue.Count * cooldown));
                            //    _mainWindow.setBanProgress(this, banindex, ActionListLenght);
                            //    _mainWindow.setETA(this, eta.ToString("g"));
                            //}

                            foreach (var channel in client.JoinedChannels)
                            {
                                client.SendMessage(channel, message);

                                string to_log = $"MT: {DateTime.Now:HH:mm:ss fff} >#{channel.Channel} - {message}";
                                Trace.WriteLine(to_log);
                                log(to_log);

                                Thread.Sleep(Cooldown);
                            }


                            index++;

                            if (entires.Count == 0 || index == entires.Count)
                            {
                                //_mainWindow.setBanProgress(this, 100, 100);
                                //_mainWindow.setETA(this, "-");
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    Cooldown += 10;
                    //_mainWindow.increaseDelay(cooldown);
                    Thread.Sleep(10);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    if (!paused)
                    {
                        //_mainWindow.ThrowError($"{e.GetType().Name} {e.Message} \n{e.StackTrace}", false);
                    }
                }
                catch (BadStateException)
                {
                    abort = true;
                }
                catch (Exception e)
                {
                    //_mainWindow.ThrowError($"{e.GetType().Name} {e.Message} \n{e.StackTrace}", false);
                }
            }, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);
        }

        public int Cooldown { get; set; }


        private void log(string toLog, bool addTimeStamp = false)
        {
            //Trace.WriteLine(toLog);
        }

        private bool running = true;


        private ObservableCollection<string> channelsJoined;

        private bool abort = false;

        private Task messageTask = null;

        private ConnectionCredentials credentials;

        public TwitchClient client { get; private set; }

        public bool ManualDisconnect { get; set; }

        private bool paused;
        private ObservableCollection<Entry> entires;
        private readonly List<string> channel;
        private readonly MainWindowViewModel owner;
    }
}
