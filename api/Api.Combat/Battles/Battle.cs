using Api.Combat.Abilities;
using Api.Combat.Battlefield;
using Api.Combat.Events;

namespace Api.Combat.Battles;

public class Battle
{
    private readonly Board _board;
    private readonly List<BattleEvent> _events = [];
    private readonly Scheduler _scheduler;

    private Battle(Team player, Team ghost, Roster roster)
    {
        _board = new Board(player, ghost);
        _scheduler = new Scheduler(_board, roster);
    }

    public static BattleResult Resolve(Team player, Team ghost, Roster roster)
    {
        return new Battle(player, ghost, roster).Run();
    }

    private BattleResult Run()
    {
        foreach (IReadOnlyList<BattleEvent> events in _scheduler.Steps())
        {
            _events.AddRange(events);
            if (MaybeOutcome() is { } outcome) return Finish(outcome);
        }

        return Finish(BattleOutcome.Draw);
    }

    private BattleResult Finish(BattleOutcome outcome) =>
        new BattleResult { Outcome = outcome, Events = _events };

    private BattleOutcome? MaybeOutcome()
    {
        bool playerEmpty = _board.IsEmpty(Side.Player);
        bool ghostEmpty = _board.IsEmpty(Side.Ghost);

        return (playerEmpty, ghostEmpty) switch
        {
            (true, false) => BattleOutcome.Loss,
            (false, true) => BattleOutcome.Win,
            (true, true) => BattleOutcome.Draw,
            (false, false) => null
        };
    }
}
