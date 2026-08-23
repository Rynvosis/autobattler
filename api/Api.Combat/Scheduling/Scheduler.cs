using Api.Combat.Abilities;
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

    void IResolutionContext.Emit(BattleEvent battleEvent) => Emit(battleEvent);

    private void RunBattleStart()
    {
        Emit(new StartEvent());
    }

    private void RunTick()
    {
        _tick++;
        _subtick = 0;

        (Unit playerHead, Unit ghostHead) = board.Heads();

        Emit(new UnitAttackEvent { Source = playerHead, Target = ghostHead, Value = playerHead.Attack });
        Emit(new UnitAttackEvent { Source = ghostHead, Target = playerHead, Value = ghostHead.Attack });

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
            Emit(new UnitDeathEvent { Target = unit });
        }

        foreach (Unit unit in dead)
        {
            board.Remove(unit);
        }
    }

    private void Emit(BattleEvent battleEvent)
    {
        _stepEvents.Add(battleEvent with { Tick = _tick, Subtick = _subtick });
        _effectsNextSubtick.AddRange(TriggerScan.Scan(board, battleEvent));
    }
}
