using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassBanToolMP.Models
{
    public class Defaults
    {
        public static readonly string[] AllowedActions = new string[]
        {
            "ban",
            "unban",
            "mod",
            "unmod",
            "block",
            "unblock",
            "vip",
            "unvip",
            "timeout",
            "slow",
            "slowoff",
            "followers",
            "followersoff",
            "subscribers",
            "subscribersoff",
            "clear",
            "uniquechat",
            "uniquechatoff",
            "emoteonly",
            "emoteonlyoff"
        };
    }
}
