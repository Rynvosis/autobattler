using Api.Combat.Battlefield;
using Api.Combat.Events;
using Api.Combat.Scopes;

namespace Api.Combat.Effects;

public sealed record ScopedEffect<TEvent> where TEvent : BattleEvent
{
    public required Effect<TEvent> Effect { get; init; }
    public required IReadOnlyList<Every<TEvent>> Scopes { get; init; }

    public IReadOnlyList<Unit> Targets(Context context, TEvent battleEvent)
    {
        HashSet<Unit> selected =
            [.. Scopes.SelectMany(scope => scope.Of(context, battleEvent, Effect.Reaches))];

        return [.. context.UnitsInIterationOrder().Where(selected.Contains)];
    }
}
