namespace Api.Runs;

public sealed record Run
{
    public required string RunId { get; init; }
    public required int Version { get; init; }
    public required int Gold { get; init; }
    public required int Tier { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IReadOnlyList<RunUnit> Units { get; init; }
}