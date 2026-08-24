namespace Api.Runs;

public static class RunTable
{
    public const string TableName = "runs";

    public const string RunId = "runId";
    public const string Version = "version";
    public const string Gold = "gold";
    public const string Tier = "tier";
    public const string ExpiresAt = "expiresAt";
    public const string Units = "units";

    public static class Unit
    {
        public const string Kind = "kind";
        public const string Attack = "attack";
        public const string Health = "health";
    }
}