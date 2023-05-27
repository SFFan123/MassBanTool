using System;
using Avalonia.Controls;
using TwitchLib.Api.Auth;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class TokenInfoDialog : Window
    {
        public TokenInfoDialog():this(null, null)
        { }

        public TokenInfoDialog(ValidateAccessTokenResponse? tokenInfo = null, double? rate = null)
        {
            if(tokenInfo != null)
                this.TokenInfo = tokenInfo;

            if(rate != null)
                this.Rate = rate;

            InitializeComponent();
            DataContext = this;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public ValidateAccessTokenResponse TokenInfo { get; set; }
        public double? Rate { get; set; }

        public string ExpireDateTime => DateTime.Now.AddSeconds(TokenInfo.ExpiresIn).ToString("F");
    }
}
