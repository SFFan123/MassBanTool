namespace MassBanTool.Models
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
        None = default,
        UserList,
        ReadFile,
        Mixed,
        Malformed
    }
}