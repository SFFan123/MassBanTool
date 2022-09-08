using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MassBanToolMP.ViewModels;
using TwitchLib.Client;
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
        private const string NoticeIDAlreadyBanned = "already_banned";
        private const string NoticeIDAdminBanAttempt = "bad_ban_admin";
        private const string NoticeIDAnonBanAttempt = "bad_ban_anon";
        private const string NoticeIDInvalidUserName = "invalid_user";
        private const string NoticeIDStaffBanAttempt = "bad_ban_staff";

        private readonly List<string> channels;
        private readonly MainWindowViewModel owner;


        private ConnectionCredentials credentials;

        private WebSocketClient customClient;

        private Regex noticeRegex = new(@"^@msg-id=(\w+) :tmi\.twitch\.tv NOTICE #(?'channels'\w+) :(\w+)",
            RegexOptions.Compiled);

        public TwitchChatClient(MainWindowViewModel owner, string user, string oauth, List<string> channels)
        {
            this.owner = owner;
            credentials = new ConnectionCredentials(user, oauth);

            this.channels = channels;

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 100,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                UseSsl = true,
                ReconnectionPolicy = new ReconnectionPolicy(1000)
            };
            customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);

            client = InitializeClient();

            RegisterClientEventHandler(client);
            
            client.Connect();
        }

        public bool IsConnected => client != null && client.IsConnected;
        
        private TwitchClient client { get; set; }
        public bool ManualDisconnect { get; set; }
        public IReadOnlyList<JoinedChannel> JoinedChannels => client.JoinedChannels;

        private TwitchClient InitializeClient()
        {
            client.Initialize(credentials);
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
            client.OnIncorrectLogin += IncorrectLogin;
            client.OnVIPsReceived += Client_OnVIPsReceived;
            client.OnModeratorsReceived += Client_OnModeratorsReceived;
            client.OnMessageThrottled += Client_OnMessageThrottled;
            client.OnConnected += ClientOnConnected;
            client.OnLeftChannel += Client_OnLeftChannel;
        }

        private void Client_OnLeftChannel(object? sender, OnLeftChannelArgs e)
        {
            owner.Client_OnLeftChannel(e.Channel);
        }

        private void ClientOnConnected(object? sender, OnConnectedArgs e)
        {
            JoinChannels(channels);
            owner.OnConnected();
        }

        private void Client_OnMessageThrottled(object? sender, OnMessageThrottledEventArgs e)
        {
            LogViewModel.Log("Message throttle reached increasing Delay.");
            owner.MessageThrottled();
        }

        private void Client_OnModeratorsReceived(object? sender, OnModeratorsReceivedArgs e)
        {
            owner.SetChannelMods(e.Channel, e.Moderators);
        }

        private void Client_OnVIPsReceived(object? sender, OnVIPsReceivedArgs e)
        {
            owner.SetChannelVIPs(e.Channel, e.VIPs);
        }

        private void IncorrectLogin(object? sender, OnIncorrectLoginArgs e)
        {
            owner.IncorrectLogin();
        }

        private void Client_OnUnaccountedFor(object? sender, OnUnaccountedForArgs e)
        {
            string msg = e.RawIRC;
            Match match = noticeRegex.Match(msg);
            if (match.Success)
            {
                switch (match.Groups[1].Value)
                {
                    case NoticeIDAlreadyBanned:
                        owner.OnUserAlreadyBanned(e.Channel, match.Groups[2].Value);
                        break;
                    case NoticeIDAdminBanAttempt:
                    case NoticeIDAnonBanAttempt:
                    case NoticeIDInvalidUserName:
                    case NoticeIDStaffBanAttempt:
                        owner.OnBadUserBan(e.Channel, match.Groups[2].Value, match.Groups[1].Value);
                        break;
                }
            }
        }

        private void OnUserBanned(object? sender, OnUserBannedArgs e)
        {
            owner.OnUserBanned(e.UserBan.Channel, e.UserBan.Username);
        }

        private Dictionary<string, int> joinFails = new();

        private void Client_OnFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
            if (!string.IsNullOrEmpty(e.Exception.Details))
            {
                LogViewModel.Log(e.Exception.Details);
                owner.FailedToJoinChannel(e.Exception.Channel, e.Exception.Details);
            }
            if (joinFails.TryGetValue(e.Exception.Channel, out int value))
            {
                joinFails[e.Exception.Channel]++;
            }
            else
            {
                joinFails.Add(e.Exception.Channel, 1);
            }

            if (value > 3)
            {
                owner.FailedToJoinChannel(e.Exception.Channel, e.Exception.Details);
                return;
            }
            //count errors and retry.
            LogViewModel.Log(e.Exception.Details);

            client.JoinChannel(e.Exception.Channel);
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
            bool isBroadcaster = e.UserState.Channel.ToLower() == client.TwitchUsername.ToLower();
            bool isMod = !isBroadcaster && e.UserState.IsModerator;
            if (isMod || isBroadcaster)
            {
                owner.AddChannelToGrid(e.UserState.Channel, isMod);
                return;
            }

            client.LeaveChannel(e.UserState.Channel);

            owner.MissingPermissions(e.UserState.Channel);
        }

        private async void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            // Make sure the client is ready before firing the query to avoid weird exceptions.
            while(!client.IsConnected)
                await Task.Delay(20);

            lock (this)
            {
                if (joinFails.ContainsKey(e.Channel))
                    joinFails.Remove(e.Channel);

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


        public void LeaveChannel(string channel)
        {
            client.LeaveChannel(channel);
        }


        public async void JoinChannels(IEnumerable<string> channels)
        {
            while (!IsConnected)
            {
                await Task.Delay(10);
            }
            ushort joins = 0;
            foreach (string channel in channels)
            {
                if (joins!= 0 && joins % 20 == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    joins = 0;
                }

                client.JoinChannel(channel);

                joins++;
            }
        }
    }
}