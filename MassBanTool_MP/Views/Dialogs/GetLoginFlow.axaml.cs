using System.Collections.Generic;
using System.Linq;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MassBanToolMP.ViewModels;
using Microsoft.CodeAnalysis.FlowAnalysis;
using DialogResult = MassBanToolMP.Helper.DialogResult;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class GetLoginFlow : Window
    {
        public GetLoginFlow()
        {
            InitializeComponent();
            this.DataContext = this;
            Result = DialogResult.Aborted;

            foreach (object o in ListBox_Scopes.Items)
            {
                if (o is KeyValuePair<string, string> keyValuePair)
                {
                    if (basicScopes.Contains(keyValuePair.Key))
                    {
                        ListBox_Scopes.SelectedItem = o;
                    }
                }
            }

        }

        private string[] basicScopes = new[] { "moderator:manage:banned_users" };


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

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            string scopes = string.Empty;

            if (CheckBox_All_Scopes.IsChecked == true)
            {
                scopes = string.Join("+", AuthScopesList);
            }
            else
            {
                foreach (object item in ListBox_Scopes.SelectedItems)
                {
                    if (item is string sc)
                    {
                        scopes = string.Join("+", scopes, sc);
                    }
                }

            }

            if (string.IsNullOrEmpty(scopes))
            {
                return;
            }
            
            var url = "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=" + Program.API.Settings.ClientId +
                      "&redirect_uri=" + HttpUtility.UrlEncode("https://twitchapps.com/tmi/") +
                      "&response_type=code&scope=" + scopes; 
            
            MainWindowViewModel.OpenUrl(url);
        }

        private async void Button_OnOKClick(object? sender, RoutedEventArgs e)
        {
            string oauthprefix = "oauth:";
            if (Token.StartsWith(oauthprefix))
                Token = Token.Remove(oauthprefix.Length);

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
    }
}
