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
            if (unit.Ability?.Fire(new Context(board, unit), battleEvent) is { } queued) effects.Add(queued);
        }

        return effects;
    }
}
