namespace MassBanTool
{
    public enum ToolStatus
    {
        Banning,
        UnBanning,
        ReadFile,
        Ready,
        Aborted,
        Disconnected,
        Paused,
        Done,
        Importing
    }

    public enum ListType
    {
        none = default,
        UserList,
        ReadFile,
        Mixed
    }
}