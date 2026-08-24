using Api.Combat.Abilities.Scopes;
using Api.Combat.Effects;
using Api.Combat.Events;

namespace Api.Combat.Abilities;

public abstract record Ability
{
    public abstract QueuedEffect? Fire(Context context, BattleEvent battleEvent);
}

public sealed record Ability<TEvent> : Ability where TEvent : BattleEvent
{
    public required Trigger<TEvent> Trigger { get; init; }
    public required Effect<TEvent> Effect { get; init; }
    public required IReadOnlyList<IEffectScope<TEvent>> Scopes { get; init; }

    public override QueuedEffect? Fire(Context context, BattleEvent battleEvent)
    {
        if (battleEvent is not TEvent typed) return null;

        if (!Trigger.Matches(typed, context)) return null;

        return new QueuedEffect<TEvent>
        {
            Effect = Effect,
            Event = typed,
            Context = context,
            Targets = ScopeResolver.Resolve(Scopes, context, typed)
        };
    }
}
