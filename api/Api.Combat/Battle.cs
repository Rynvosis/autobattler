using Api.Combat.Events;
using Api.Combat.Scheduling;

namespace Api.Combat;

public class Battle
{
    private readonly Board _board;
    private readonly Scheduler _scheduler;
    private readonly List<BattleEvent> _events = [];

    private Battle(Team player, Team ghost)
    {
        _board = new Board(player, ghost);
        _scheduler = new Scheduler(_board);
    }

    public static BattleResult Resolve(Team player, Team ghost) =>
        new Battle(player, ghost).Run();

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
