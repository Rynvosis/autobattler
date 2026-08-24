using Api.Combat.Abilities;
using Api.Combat.Battlefield;
using Api.Combat.Effects;
using Api.Combat.Events;

namespace Api.Combat.Battles;

public static class TriggerScan
{
    public static IReadOnlyList<QueuedEffect> Scan(Board board, Roster roster, BattleEvent battleEvent)
    {
        List<QueuedEffect> effects = [];

        foreach ((Unit unit, Position _) in board.UnitsInIterationOrder())
        {
            if (roster.For(unit.Kind) is { } ability)
                effects.AddRange(ability.Fire(new Context(board, unit), battleEvent));
        }

        return effects;
    }
}
