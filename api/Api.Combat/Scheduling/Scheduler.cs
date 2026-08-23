using Api.Combat.Effects;
using Api.Combat.Events;

namespace Api.Combat.Scheduling;

public class Scheduler(Board board) : IResolutionContext
{
    private const int TickCap = 64;
    private const int SubtickCap = 32;

    private readonly List<BattleEvent> _stepEvents = [];
    private List<QueuedEffect> _effectsThisSubtick = [];
    private List<QueuedEffect> _effectsNextSubtick = [];
    private int _tick;
    private int _subtick;

    public IEnumerable<IReadOnlyList<BattleEvent>> Steps()
    {
        _stepEvents.Clear();
        RunBattleStart();
        Drain();
        yield return [.. _stepEvents];

        for (int i = 0; i < TickCap; i++)
        {
            _stepEvents.Clear();
            RunTick();
            Drain();
            yield return [.. _stepEvents];
        }
    }

    void IResolutionContext.Emit(EventKind kind, Unit? source, Unit? target, int? value) =>
        Emit(kind, source, target, value);

    private void RunBattleStart()
    {
        Emit(EventKind.OnStart);
    }

    private void RunTick()
    {
        _tick++;
        _subtick = 0;

        (Unit playerHead, Unit ghostHead) = board.Heads();

        Emit(EventKind.OnUnitAttack, playerHead, ghostHead, playerHead.Attack);
        Emit(EventKind.OnUnitAttack, ghostHead, playerHead, ghostHead.Attack);

        _effectsNextSubtick.Add(new QueuedEffect
        {
            Effect = new Damage { Value = playerHead.Attack },
            Source = playerHead,
            Targets = [ghostHead]
        });
        _effectsNextSubtick.Add(new QueuedEffect
        {
            Effect = new Damage { Value = ghostHead.Attack },
            Source = ghostHead,
            Targets = [playerHead]
        });
    }

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
        List<Unit> dead = [];

        foreach ((Unit unit, Position _) in board.UnitsInIterationOrder())
        {
            if (unit.Health > 0) continue;
            unit.Dead = true;
            dead.Add(unit);
        }

        foreach (Unit unit in dead)
        {
            Emit(EventKind.OnUnitFaint, target: unit);
        }

        foreach (Unit unit in dead)
        {
            board.Remove(unit);
        }
    }

    private void Emit(EventKind kind, Unit? source = null, Unit? target = null, int? value = null) =>
        _stepEvents.Add(new BattleEvent
        {
            Kind = kind,
            Tick = _tick,
            Subtick = _subtick,
            Source = source?.Id,
            Target = target?.Id,
            Value = value,
        });
}
