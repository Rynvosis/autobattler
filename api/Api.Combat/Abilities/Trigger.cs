using Api.Combat.Abilities.Scopes;
using Api.Combat.Events;

namespace Api.Combat.Abilities;

public abstract record Trigger
{
    public abstract bool Matches(BattleEvent battleEvent, TriggerContext context);
}

public sealed record RoundTrigger<TEvent> : Trigger where TEvent : RoundEvent
{
    public override bool Matches(BattleEvent battleEvent, TriggerContext context) => battleEvent is TEvent;
}

public sealed record TargetTrigger<TEvent> : Trigger where TEvent : UnitEvent
{
    public required IReadOnlyList<ITriggerScope> Scopes { get; init; }

    public override bool Matches(BattleEvent battleEvent, TriggerContext context) =>
        battleEvent is TEvent unitEvent && ScopeResolver.Resolve(Scopes, context).Contains(unitEvent.Target);
}

public sealed record SourceTrigger<TEvent> : Trigger where TEvent : SourcedUnitEvent
{
    public required IReadOnlyList<ITriggerScope> Scopes { get; init; }

    public override bool Matches(BattleEvent battleEvent, TriggerContext context)
    {
        return battleEvent is TEvent unitEvent && ScopeResolver.Resolve(Scopes, context).Contains(unitEvent.Source);
    }
}
