using Api.Combat.Abilities;

namespace Api.Combat;

public class Unit
{
    public required int Id { get; init; }
    public required int Attack { get; init; }
    public required int Health { get; set; }
    public required Ability? Ability { get; init; }

    public bool Dead { get; set; }
}
