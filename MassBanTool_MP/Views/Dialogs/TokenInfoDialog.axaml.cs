using System;
using Avalonia.Controls;
using TwitchLib.Api.Auth;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class TokenInfoDialog : Window
    {
        public TokenInfoDialog():this(null)
        { }

        public TokenInfoDialog(ValidateAccessTokenResponse? tokenInfo = null)
        {
            if(tokenInfo != null)
                this.TokenInfo = tokenInfo;

            InitializeComponent();
            DataContext = this;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public ValidateAccessTokenResponse TokenInfo { get; set; }

        public string ExpireDateTime => DateTime.Now.AddSeconds(TokenInfo.ExpiresIn).ToString("F");
    }
}
