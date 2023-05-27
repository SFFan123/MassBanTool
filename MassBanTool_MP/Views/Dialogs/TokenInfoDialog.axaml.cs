using System;
using Avalonia.Controls;
using TwitchLib.Api.Auth;

namespace MassBanToolMP.Views.Dialogs
{
    public partial class TokenInfoDialog : Window
    {
        public TokenInfoDialog():this(null)
        { }

        public TokenInfoDialog(ValidateAccessTokenResponse? tokenInfo = null, DateTime resultTime = default , double? rate = null)
        {
            if(tokenInfo != null)
                this.TokenInfo = tokenInfo;

            if(rate != null)
                this.Rate = rate;

            if (resultTime != default)
                ResultTime = resultTime;

            InitializeComponent();
            DataContext = this;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public ValidateAccessTokenResponse TokenInfo { get; set; }
        public double? Rate { get; set; }

        public DateTime ResultTime { get; set; }

        public string ExpireDateTime => ResultTime.AddSeconds(TokenInfo.ExpiresIn).ToString("F");
    }
}
