using Api.Combat.Events;

namespace Api.Combat;

public class Battle : IBattleContext
{
    private const int TickCap = 64;
    private const int SubtickCap = 32;

    private readonly Team _player;
    private readonly Team _ghost;
    private readonly List<BattleEvent> _events = [];
    private List<QueuedEffect> _wave = [];
    private List<QueuedEffect> _nextWave = [];
    private int _tick;
    private int _subtick;

    private Battle(Team player, Team ghost)
    {
        _player = player;
        _ghost = ghost;
    }

    public static BattleResult Resolve(Team player, Team ghost) =>
        new Battle(player, ghost).Run();

    private static IEnumerable<ScheduleStep> Schedule() =>
        [ScheduleStep.BattleStart, .. Enumerable.Repeat(ScheduleStep.Tick, TickCap)];

    private BattleResult Run()
    {
        foreach (ScheduleStep step in Schedule())
        {
            Execute(step);

            ResolveDeaths();

            if (MaybeOutcome() is { } outcome) return Finish(outcome);
        }

        return Finish(BattleOutcome.Draw);
    }

    private void Execute(ScheduleStep step)
    {
        switch (step)
        {
            case ScheduleStep.BattleStart: RunBattleStart(); break;
            case ScheduleStep.Tick: RunTick(); break;
        }
    }

    private BattleResult Finish(BattleOutcome outcome) =>
        new BattleResult { Outcome = outcome, Events = _events };

    private BattleOutcome? MaybeOutcome()
    {
        bool playerEmpty = _player.IsEmpty;
        bool ghostEmpty = _ghost.IsEmpty;

        return (playerEmpty, ghostEmpty) switch
        {
            (true, false) => BattleOutcome.Loss,
            (false, true) => BattleOutcome.Win,
            (true, true) => BattleOutcome.Draw,
            (false, false) => null
        };
    }

    private void RunBattleStart()
    {
        Emit(EventKind.OnStart);
    }

    private void RunTick()
    {
        _tick++;
        _subtick = 0;
        
        Unit playerHead = _player.Head;
        Unit ghostHead = _ghost.Head;

        Emit(EventKind.OnUnitAttack, playerHead, ghostHead, playerHead.Attack);
        Emit(EventKind.OnUnitAttack, ghostHead, playerHead, ghostHead.Attack);

        //todo: queue these into the next wave instead of applying them here
        _subtick++;
        Damage playerDamage = new()
        {
            Source = playerHead,
            Targets = [ghostHead],
            Value = playerHead.Attack
        };
        playerDamage.Apply(this);
        Damage ghostDamage = new()
        {
            Source = ghostHead,
            Targets = [playerHead],
            Value = ghostHead.Attack
        };
        ghostDamage.Apply(this);
    }

    void IBattleContext.Emit(EventKind kind, Unit? source, Unit? target, int? value) =>
        Emit(kind, source, target, value);

    private void RunSubticks()
    {
    }

    private void ResolveDeaths()
    {
        List<(Unit unit, Position position)> dead = [];

        foreach ((Unit unit, Position position) in UnitsInIterationOrder())
        {
            if (unit.Health > 0) continue;
            unit.Dead = true;
            dead.Add((unit, position));
        }

        foreach ((Unit unit, Position position) in dead)
        {
            Emit(EventKind.OnUnitFaint, target: unit);
            Team team = TeamFor(position.Side);
            team.Remove(unit);
        }
    }

    private void Emit(EventKind kind, Unit? source = null, Unit? target = null, int? value = null) =>
        _events.Add(new BattleEvent
        {
            Kind = kind,
            Tick = _tick,
            Subtick = _subtick,
            Source = source?.Id,
            Target = target?.Id,
            Value = value,
        });

    private IReadOnlyList<(Unit unit, Position position)> UnitsInIterationOrder()
    {
        List<(Unit, Position)> result = [];
        int max = Math.Max(_player.Count, _ghost.Count);

        for (int i = 0; i < max; i++)
        {
            if (i < _player.Count) result.Add((_player.Units[i], new Position(Side.Player, i)));
            if (i < _ghost.Count) result.Add((_ghost.Units[i], new Position(Side.Ghost, i)));
        }

        return result;
    }

    private Team TeamFor(Side side) => side == Side.Player ? _player : _ghost;
}