using Api.Combat.Events;

namespace Api.Battles;

public static class EventRecords
{
    public static IReadOnlyList<EventRecord> From(IReadOnlyList<BattleEvent> events)
    {
        return [.. events.Select(From)];
    }

    public static EventRecord From(BattleEvent battleEvent)
    {
        return RecordFor(battleEvent) with
        {
            Tick = battleEvent.Tick,
            Subtick = battleEvent.Subtick,
            Cause = From(battleEvent.Cause)
        };
    }

    private static CauseRecord From(Cause cause)
    {
        return new CauseRecord { Kind = cause.Kind, Unit = cause.Owner?.Id };
    }

    private static EventRecord RecordFor(BattleEvent battleEvent)
    {
        return battleEvent switch
        {
            StartEvent => new StartRecord(),
            UnitDeathEvent death => new UnitDeathRecord { Target = death.Target.Id },
            UnitKillEvent kill => new UnitKillRecord { Source = kill.Source.Id, Target = kill.Target.Id },
            UnitAttackEvent attack => new UnitAttackRecord
            {
                Source = attack.Source.Id,
                Target = attack.Target.Id,
                Value = attack.Value
            },
            UnitHurtEvent hurt => new UnitHurtRecord
            {
                Source = hurt.Source.Id,
                Target = hurt.Target.Id,
                Value = hurt.Value
            },
            UnitAttackChangeEvent change => new UnitAttackChangeRecord
            {
                Source = change.Source.Id,
                Target = change.Target.Id,
                Value = change.Value
            },
            UnitHealthChangeEvent change => new UnitHealthChangeRecord
            {
                Source = change.Source.Id,
                Target = change.Target.Id,
                Value = change.Value
            },
            _ => throw new NotSupportedException($"No record for the battle event {battleEvent.GetType().Name}")
        };
    }
}
