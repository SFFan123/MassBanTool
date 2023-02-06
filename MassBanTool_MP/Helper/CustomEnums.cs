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


    public enum ReadFileOperation : ushort
    {
        // User specific
        Block, // self only - user:manage:blocked_users
        UnBlock, // self only - user:manage:blocked_users
        Vip, // self only - channel:manage:vips
        UnVip, // self only - channel:manage:vips
        Mod, // self only - channel:manage:moderators
        UnMod, // self only - channel:manage:moderators

        // Mod
        Timeout, // moderator:manage:banned_users
        Ban, // moderator:manage:banned_users
        UnBan, // moderator:manage:banned_users
        UnTimeout, // moderator:manage:banned_users
        AddBlockedTerm, // moderator:manage:blocked_terms
        RemoveBlockedTerm, // moderator:manage:blocked_terms

        // Room
        Clear, // moderator:manage:chat_messages
        Slow, // moderator:manage:chat_settings
        SlowOff, // moderator:manage:chat_settings
        Followers, // moderator:manage:chat_settings
        FollowersOff, // moderator:manage:chat_settings
        Subscribers, // moderator:manage:chat_settings
        SubscribersOff, // moderator:manage:chat_settings
        Uniquechat, // moderator:manage:chat_settings
        UniquechatOff, // moderator:manage:chat_settings
        Emoteonly, // moderator:manage:chat_settings
        EmoteonlyOff, // moderator:manage:chat_settings
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
