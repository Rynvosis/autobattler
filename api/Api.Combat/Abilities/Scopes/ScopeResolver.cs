using Api.Combat.Events;

namespace Api.Combat.Abilities.Scopes;

public static class ScopeResolver
{
    public static IReadOnlyList<Unit> Resolve(IReadOnlyList<ITriggerScope> scopes, Context context)
    {
        return [.. InBoardOrder(context, scopes.SelectMany(scope => scope.Resolve(context)))];
    }

    public static IReadOnlyList<Unit> Resolve<TEvent>(
        IReadOnlyList<IEffectScope<TEvent>> scopes,
        Context context,
        TEvent battleEvent)
        where TEvent : BattleEvent
    {
        return
        [
            .. InBoardOrder(context, scopes.SelectMany(scope => scope.Resolve(context, battleEvent)))
                .Where(unit => !unit.Dead)
        ];
    }

    private static IEnumerable<Unit> InBoardOrder(Context context, IEnumerable<Unit> units)
    {
        HashSet<Unit> selected = [.. units];

        return context.Board.UnitsInIterationOrder().Select(entry => entry.unit).Where(selected.Contains);
    }
}
