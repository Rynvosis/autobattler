using Api.Combat.Units;

namespace Api.Teams;

public sealed record TeamUnit
{
    public required Kind Kind { get; init; }
    public required int Attack { get; init; }
    public required int Health { get; init; }
}
