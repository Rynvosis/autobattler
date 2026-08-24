using Api.Combat.Abilities;
using Api.Combat.Units;

namespace Api.Content;

public sealed record UnitDefinition
{
    public required Kind Kind { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required int Attack { get; init; }
    public required int Health { get; init; }
    public required int Tier { get; init; }
    public required Ability? Ability { get; init; }
}
