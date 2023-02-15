using System;
using System.Collections.Generic;

namespace MassBanToolMP.Helper
{
    public enum WorkingMode
    {
        Ban,
        Unban,
        Readfile
    }

    public enum ListType
    {
        None = default,
        UserList,
        ReadFile,
        Mixed,
        Malformed,
    }

    /// <summary>
    /// Action - required scope, own channel only
    /// </summary>
    public static class ReadFileOperationScopeMapping
    {
        private static Tuple<string, bool>
            chatManage = new Tuple<string, bool>("moderator:manage:chat_settings", false);


        public static Dictionary<ReadFileOperation, Tuple<string, bool>> OperationScopeMapping =
            new Dictionary<ReadFileOperation, Tuple<string, bool>>()
            {
                {ReadFileOperation.Block,             new Tuple<string,bool>("user:manage:blocked_users", true)},
                {ReadFileOperation.UnBlock,           new Tuple<string,bool>("user:manage:blocked_users", true)},

                {ReadFileOperation.Vip,               new Tuple<string,bool>("channel:manage:vips", true)},
                {ReadFileOperation.UnVip,             new Tuple<string,bool>("channel:manage:vips", true)},
                {ReadFileOperation.Mod,               new Tuple<string,bool>("channel:manage:moderators", true)},
                {ReadFileOperation.UnMod,             new Tuple<string,bool>("channel:manage:moderators", true)},

                {ReadFileOperation.Timeout,           new Tuple<string,bool>("moderator:manage:banned_users", false)},
                {ReadFileOperation.Ban,               new Tuple<string,bool>("moderator:manage:banned_users", false)},
                {ReadFileOperation.UnBan,             new Tuple<string,bool>("moderator:manage:banned_users", false)},
                {ReadFileOperation.UnTimeout,         new Tuple<string,bool>("moderator:manage:banned_users", false)},
                {ReadFileOperation.AddBlockedTerm,    new Tuple<string,bool>("moderator:manage:blocked_terms", false)},
                {ReadFileOperation.RemoveBlockedTerm, new Tuple<string,bool>("moderator:manage:blocked_terms", false)},
                {ReadFileOperation.Clear,             new Tuple<string,bool>("moderator:manage:chat_messages", false)},

                {ReadFileOperation.Slow,              chatManage},
                {ReadFileOperation.SlowOff,           chatManage},
                {ReadFileOperation.Followers,         chatManage},
                {ReadFileOperation.FollowersOff,      chatManage},
                {ReadFileOperation.Subscribers,       chatManage},
                {ReadFileOperation.SubscribersOff,    chatManage},
                {ReadFileOperation.Uniquechat,        chatManage},
                {ReadFileOperation.UniquechatOff,     chatManage},
                {ReadFileOperation.Emoteonly,         chatManage},
                {ReadFileOperation.EmoteonlyOff,      chatManage},
            };
    }
    

    public enum ReadFileOperation : ushort
    {
        // User specific
        Block,
        UnBlock,
        Vip,
        UnVip,
        Mod,
        UnMod,

        // Mod
        Timeout,
        Ban,
        UnBan,
        UnTimeout,
        AddBlockedTerm,
        RemoveBlockedTerm,

        // Room
        Clear,
        Slow,
        SlowOff,
        Followers,
        FollowersOff,
        Subscribers,
        SubscribersOff,
        Uniquechat,
        UniquechatOff,
        Emoteonly,
        EmoteonlyOff,
    }

    public enum NotSupportedOperations : ushort
    {
        Monitor, // Not supported by API
        UnMonitor, // Not supported by API
        Restrict, // Not supported by API
        UnRestrict, // Not supported by API
    }

    public enum DialogResult
    {
        Aborted,
        OK,
    }
}
