namespace Api.Combat.Abilities.Scopes;

public record TriggerContext(Board Board, Unit Owner);

public record EffectContext(Board Board, Unit Owner, Unit? EventSource, Unit? EventTarget)
    : TriggerContext(Board, Owner);
