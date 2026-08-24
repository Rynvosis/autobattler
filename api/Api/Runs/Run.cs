using Api.Teams;

namespace Api.Runs;

public sealed record Run
{
    public required string RunId { get; init; }
    public required int Version { get; init; }
    public required int Victories { get; init; }
    public required int Gold { get; init; }
    public required int Stage { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IReadOnlyList<TeamUnit> Units { get; init; }

    public bool Finished => Stage > Economy.TotalStages;
}