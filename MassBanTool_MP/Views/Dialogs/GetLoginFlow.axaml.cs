using System.Collections.Generic;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MassBanToolMP.ViewModels;
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
        }

        public List<string> AuthScopesList
        {
            get => new List<string>()
            {
                "moderator:manage:banned_users",
                "moderation:read",
                "moderator:manage:automod_settings",
                "moderator:manage:blocked_terms",
                "moderator:manage:chat_settings",
                "moderator:manage:chat_messages",

                //PubSub
                //"chat:read",
                "channel:moderate",
                
                // Personal
                "channel:manage:moderators",
                "channel:manage:vips",
                "user:manage:blocked_users"

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
            }
            else
            {
                await MessageBox.Show("Token was rejected by Twitch", "Token failure");
            }
        }
    }
}
