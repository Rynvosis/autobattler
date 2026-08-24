using Api.Combat.Battlefield;
using Api.Combat.Events;

namespace Api.Combat.Effects;

public abstract record QueuedEffect
{
    public required Context Context { get; init; }
    public required IReadOnlyList<Unit> Targets { get; init; }

    public abstract IReadOnlyList<BattleEvent> Apply();
}

public sealed record QueuedEffect<TEvent> : QueuedEffect where TEvent : BattleEvent
{
    public required Effect<TEvent> Effect { get; init; }
    public required TEvent Event { get; init; }

    public override IReadOnlyList<BattleEvent> Apply()
    {
        return
        [
            .. Targets
                .Where(target => !target.Dead)
                .SelectMany(target => Effect.Apply(Context, Event, target))
        ];
    }
}
