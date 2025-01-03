using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MassBanToolMP.Helper;
using WebAPI;
using DialogResult = MassBanToolMP.Helper.DialogResult;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class GetLoginFlow : Window, ITokenReceiver
    {
        public GetLoginFlow()
        {
            InitializeComponent();
            this.DataContext = this;
            Result = DialogResult.Aborted;

            foreach (object o in ListBox_Scopes.Columns)
            {
                if (o is KeyValuePair<string, string> keyValuePair)
                {
                    if (basicScopes.Contains(keyValuePair.Key))
                    {
                        ListBox_Scopes.SelectedItems.Add(o);
                    }
                }
            }

        }

        private string[] basicScopes = new[] { "moderator:manage:banned_users"};

        public Dictionary<string, string> AuthScopesList
        {
            get => new Dictionary<string, string>()
            {
                {"moderator:manage:banned_users", "Enables Ban, UnBan, Timeout, Untimeout"},
                {"moderator:manage:blocked_terms", "Enables Readfile Operations: AddBlockedTerm, RemoveBlockedTerm"},
                {"moderator:manage:chat_settings", "Enables Readfile Operations: Slow, Slowoff, Followers, FollowersOff, Subscribers, SubscribersOff, Uniquechat, UniquechatOff, Emoteonly, EmoteonlyOff"},
                {"moderator:manage:chat_messages", "Enables Readfile Operations: Clear"},

                //PubSub
                {"channel:moderate","Enables reading of Moderation Actions in PubSub/Chat"},
                
                // Personal
                {"channel:manage:moderators","Enables Readfile Operations: Mod, UnMod"},
                {"channel:manage:vips","Enables Readfile Operations: Vip, UnVip"},
                {"user:manage:blocked_users", "Enables ReadfileO perations: Block, Unblock"},
            };
        }

        public string Token { get; set; }= string.Empty;
        public string Username { get; set; }= string.Empty;
        public string UserId { get; set; }= string.Empty;

        public DialogResult Result;

        private Task? WebHost;
        private CancellationTokenSource cts;

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            List<string> scopeList = new List<string>();
            
            if (CheckBox_All_Scopes.IsChecked == true)
            {
                scopeList.AddRange(AuthScopesList.Select(x => x.Key));
            }
            else
            {
                foreach (object item in ListBox_Scopes.SelectedItems)
                {
                    if (item is KeyValuePair<string,string> sc)
                    {
                        scopeList.Add(sc.Key);
                    }
                }
            }
            string scopes = string.Join("+", scopeList);

            if (string.IsNullOrEmpty(scopes))
            {
                return;
            }
            
            var url = "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=" + Program.API.Settings.ClientId +
                      "&redirect_uri=" + HttpUtility.UrlEncode("http://localhost:23715/token") +
                      "&response_type=code&scope=" + scopes;


            cts = new CancellationTokenSource();

            if (!WebHost?.IsCompleted ?? false)
            {
                cts.Cancel();
                WebHost = null;
            }

            WebHost = WebAPI.Program.StartAsync(cts.Token, this);


            HttpHelper.OpenUrl(url);
        }

        private async void Button_OnOKClick(object? sender, RoutedEventArgs e)
        {
            string oauthprefix = "oauth:";
            if (Token.StartsWith(oauthprefix))
                Token = Token.Substring(oauthprefix.Length);

            var res = await Program.API.Auth.ValidateAccessTokenAsync( "Bearer " +  Token );
            
            if ((res != null))
            {
                Username = res.Login;
                UserId = res.UserId;
                Result = DialogResult.OK;
                Close();
            }
            else
            {
                await MessageBox.Show("Token was rejected by Twitch", "Token failure");
            }
        }

        public async void ReceiveToken(string token)
        {
            cts.CancelAsync();
            WebHost = null;
            if (Dispatcher.UIThread.CheckAccess())
            {
               InternalReceiveToken(token);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() => InternalReceiveToken(token));
            }
        }

        private async void InternalReceiveToken(string token)
        {
            try
            {
                Token = token;
                var res = await Program.API.Auth.ValidateAccessTokenAsync( "Bearer " +  Token );
            
                if ((res != null))
                {
                    Username = res.Login;
                    UserId = res.UserId;
                    Result = DialogResult.OK;
                    Close();
                }
                else
                {
                    await MessageBox.Show("Token was rejected by Twitch", "Token failure");
                }
            }
            catch (Exception e)
            {
                // TODO
            }
        }
    }
}
