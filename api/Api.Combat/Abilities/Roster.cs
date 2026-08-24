namespace Api.Combat.Abilities;

public sealed record Roster
{
    public static readonly Roster Empty = new() { Abilities = new Dictionary<Kind, Ability>() };

    public required IReadOnlyDictionary<Kind, Ability> Abilities { get; init; }

    public static Roster Of(params (Kind Kind, Ability Ability)[] entries)
    {
        return new Roster { Abilities = entries.ToDictionary(entry => entry.Kind, entry => entry.Ability) };
    }

    public Ability? For(Kind kind)
    {
        return Abilities.GetValueOrDefault(kind);
    }
}
