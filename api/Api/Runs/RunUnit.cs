using Api.Combat.Units;

namespace Api.Runs;

public sealed record RunUnit
{
    public required Kind Kind { get; init; }
    public required int Attack { get; init; }
    public required int Health { get; init; }
}