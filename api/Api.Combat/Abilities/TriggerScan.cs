using Api.Combat.Abilities.Scopes;
using Api.Combat.Effects;
using Api.Combat.Events;

namespace Api.Combat.Abilities;

public static class TriggerScan
{
    public static IReadOnlyList<QueuedEffect> Scan(Board board, BattleEvent battleEvent)
    {
        List<QueuedEffect> effects = [];

        foreach ((Unit unit, Position _) in board.UnitsInIterationOrder())
        {
            if (unit.Ability is not { } ability) continue;
            if (!ability.Trigger.Matches(battleEvent, new TriggerContext(board, unit))) continue;

            IReadOnlyList<Unit> targets =
                ScopeResolver.Resolve(ability.Scopes, battleEvent.ContextFor(board, unit));

            effects.Add(new QueuedEffect
            {
                Effect = ability.Effect,
                Source = unit,
                Targets = targets
            });
        }

        return effects;
    }
}