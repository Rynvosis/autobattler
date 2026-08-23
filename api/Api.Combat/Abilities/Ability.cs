using Api.Combat.Abilities.Scopes;
using Api.Combat.Effects;

namespace Api.Combat.Abilities;

public record Ability
{
    public required Trigger Trigger { get; init; }
    public required Effect Effect { get; init; }
    public required IReadOnlyList<IEffectScope> Scopes { get; init; }
}
