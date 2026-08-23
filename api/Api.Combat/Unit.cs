using Api.Combat.Abilities;

namespace Api.Combat;

public class Unit
{
    private int? _health;

    public required int Id { get; init; }
    public required int Attack { get; init; }
    public required int MaxHealth { get; init; }
    public required Ability? Ability { get; init; }

    public int Health
    {
        get => _health ?? MaxHealth;
        set => _health = value;
    }

    public bool Dead { get; set; }
}
