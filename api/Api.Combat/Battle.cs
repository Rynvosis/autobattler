using Api.Combat.Events;

namespace Api.Combat;

public class Battle
{
    private const int TickCap = 64;

    private readonly Team _player;
    private readonly Team _ghost;
    private readonly Dictionary<int, Unit> _unitsById;
    private readonly List<BattleEvent> _events = [];
    private int _tick;
    private int _subtick;

    private Battle(Team player, Team ghost)
    {
        _player = player;
        _ghost = ghost;
        _unitsById = player.Units.Concat(ghost.Units).ToDictionary(unit => unit.Id);
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

        //manual attack logic, todo: queue into first subtick when implement subtick
        Emit(EventKind.OnUnitAttack, playerHead.Id, ghostHead.Id, playerHead.Attack);
        Emit(EventKind.OnUnitAttack, ghostHead.Id, playerHead.Id, ghostHead.Attack);
        
        _subtick++;
        playerHead.Health -= ghostHead.Attack;
        ghostHead.Health -= playerHead.Attack;
        Emit(EventKind.OnUnitHurt, playerHead.Id, ghostHead.Id, playerHead.Attack);
        Emit(EventKind.OnUnitHurt, ghostHead.Id, playerHead.Id, ghostHead.Attack);

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
            Emit(EventKind.OnUnitFaint, target: unit.Id);
            Team team = TeamFor(position.Side);
            team.Remove(unit);
        }
    }

    private void Emit(EventKind kind, int? source = null, int? target = null, int? value = null) =>
        _events.Add(new BattleEvent
        {
            Kind = kind,
            Tick = _tick,
            Subtick = _subtick,
            Source = source,
            Target = target,
            Value = value,
        });

    private Unit? Find(int id) => _unitsById.GetValueOrDefault(id);

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