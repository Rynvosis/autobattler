using Api.Teams;

namespace Api.Ghosts;

public sealed record Ghost
{
    public required int Stage { get; init; }
    public required string RunId { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IReadOnlyList<TeamUnit> Units { get; init; }
}
