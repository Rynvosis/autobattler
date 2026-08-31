using Api.Combat.Battlefield;
using Api.Combat.Effects;
using Api.Combat.Events;

namespace Api.Combat.Abilities;

public abstract record Ability
{
    public abstract IReadOnlyList<QueuedEffect> Fire(Context context, BattleEvent battleEvent);
}

public sealed record Ability<TEvent> : Ability where TEvent : BattleEvent
{
    public required Trigger<TEvent> Trigger { get; init; }
    public required IReadOnlyList<ScopedEffect<TEvent>> Effects { get; init; }

    public override IReadOnlyList<QueuedEffect> Fire(Context context, BattleEvent battleEvent)
    {
        if (battleEvent is not TEvent typed) return [];

        if (!Trigger.Matches(typed, context)) return [];

        return
        [
            .. Effects.Select(scoped => new QueuedEffect<TEvent>
            {
                Effect = scoped.Effect,
                Event = typed,
                Context = context,
                Targets = scoped.Targets(context, typed),
                Cause = Cause.Ability(context.Owner)
            })
        ];
    }
}