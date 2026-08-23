using Api.Combat.Events;

namespace Api.Combat;

public class Battle : IBattleContext
{
    private const int TickCap = 64;
    private const int SubtickCap = 32;

    private readonly Team _player;
    private readonly Team _ghost;
    private readonly List<BattleEvent> _events = [];
    private List<QueuedEffect> _effectsThisSubtick = [];
    private List<QueuedEffect> _effectsNextSubtick = [];
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
            _subtick = 0;
            Execute(step);
            Drain();

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
        
        Unit playerHead = _player.Head;
        Unit ghostHead = _ghost.Head;

        Emit(EventKind.OnUnitAttack, playerHead, ghostHead, playerHead.Attack);
        Emit(EventKind.OnUnitAttack, ghostHead, playerHead, ghostHead.Attack);

        _effectsNextSubtick.Add(new Damage()
        {
            Source = playerHead,
            Targets = [ghostHead],
            Value = playerHead.Attack
        });
        _effectsNextSubtick.Add(new Damage()
        {
            Source = ghostHead,
            Targets = [playerHead],
            Value = ghostHead.Attack
        });
    }

    void IBattleContext.Emit(EventKind kind, Unit? source, Unit? target, int? value) =>
        Emit(kind, source, target, value);

    private void Drain()
    {
        while (_effectsNextSubtick.Count > 0 && _subtick < SubtickCap)
        {
            _subtick++;
            _effectsThisSubtick = _effectsNextSubtick;
            _effectsNextSubtick = [];

            foreach (QueuedEffect effect in _effectsThisSubtick) effect.Apply(this);
            ResolveDeaths();
        }

        _effectsThisSubtick = [];
        _effectsNextSubtick = [];
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