namespace Api.Combat.Events;

public abstract record BattleEvent
{
    public int Tick { get; init; }
    public int Subtick { get; init; }
}

public abstract record RoundEvent : BattleEvent;

public abstract record UnitEvent : BattleEvent
{
    public required Unit Target { get; init; }
}

public sealed record StartEvent : RoundEvent;

public sealed record UnitAttackEvent : UnitEvent
{
    public required Unit Source { get; init; }
    public required int Value { get; init; }
}

public sealed record UnitHurtEvent : UnitEvent
{
    public required Unit Source { get; init; }
    public required int Value { get; init; }
}

public sealed record UnitFaintEvent : UnitEvent;
