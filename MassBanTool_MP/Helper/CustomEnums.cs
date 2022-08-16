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
}
