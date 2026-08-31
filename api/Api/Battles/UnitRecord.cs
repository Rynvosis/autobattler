using Api.Combat.Units;

namespace Api.Battles;

public sealed record UnitRecord
{
    public required int Id { get; init; }
    public required Kind Kind { get; init; }
    public required int Attack { get; init; }
    public required int Health { get; init; }
}