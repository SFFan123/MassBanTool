using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MassBanToolMP.ViewModels;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace MassBanToolMP.Models
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

        private TwitchClient InitializeClient()
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 100,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                UseSsl = true,
                ReconnectionPolicy = new ReconnectionPolicy(1000)
            };
            customClient = new WebSocketClient(clientOptions);
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
            client.OnFailureToReceiveJoinConfirmation += Client_OnFailureToReceiveJoinConfirmation;
            client.OnUserBanned += OnUserBanned;
            client.OnUnaccountedFor += Client_OnUnaccountedFor;
        }

        private void Client_OnUnaccountedFor(object? sender, OnUnaccountedForArgs e)
        {
            string msg = e.RawIRC;
            Match match = alreadyBanned.Match(msg);
            if (match.Success)
            {
                owner.OnUserAlreadyBanned(e.Channel, match.Groups[1].Value);
            }
        }

        private void OnUserBanned(object? sender, OnUserBannedArgs e)
        {
            owner.OnUserBanned(e.UserBan.Channel, e.UserBan.Username);
        }

        private void Client_OnFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
            LogViewModel.Log(e.Exception.Details);
            owner.FailedToJoinChannel(e.Exception.Channel);
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
            client.LeaveChannel(e.UserState.Channel);

            owner.MissingPermissions(e.UserState.Channel);
        }

        private async void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            // Make sure the client is ready before firing the query to avoid weird exceptions.
            await Task.Delay(100);
            lock (this)
            {
                if (client.JoinedChannels.Count > 0)
                {
                    client.GetChannelModerators(e.Channel);
                    client.GetVIPs(e.Channel);
                }
            }
        }

        private void Client_OnIncorrectLogin(object? sender, OnIncorrectLoginArgs e)
        {
            client.Disconnect();
            customClient.Dispose();
        }

        private void Client_OnLog(object? sender, OnLogArgs e)
        {
            string to_log = $"{e.DateTime}: {e.BotUsername} - {e.Data}";
            LogViewModel.Log(to_log, nameof(TwitchChatClient));
            Trace.WriteLine(to_log);
        }

        public void SendMessage(JoinedChannel channel, string message)
        {
            client.SendMessage(channel, message);
        }

        
        private ConnectionCredentials credentials;

        private WebSocketClient customClient;

        public TwitchClient client { get; private set; }
        public bool ManualDisconnect { get; set; }
        private readonly List<string> channel;
        private readonly MainWindowViewModel owner;
        private Regex alreadyBanned = new (@"^@msg-id=already_banned :tmi\.twitch\.tv NOTICE #(?'channel'\w+) :(\w+)", RegexOptions.Compiled);
    }
}
