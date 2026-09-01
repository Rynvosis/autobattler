namespace Api.Combat.Events;

public abstract record BattleEvent
{
    public int Tick { get; init; }
    public int Subtick { get; init; }
    public Cause Cause { get; init; }
}

public abstract record RoundEvent : BattleEvent;

public abstract record UnitEvent : BattleEvent
{
    public required Unit Target { get; init; }
}

public abstract record SourcedUnitEvent : UnitEvent
{
    public required Unit Source { get; init; }
}

public sealed record StartEvent : RoundEvent;

public sealed record UnitAttackEvent : SourcedUnitEvent
{
    public required int Value { get; init; }
}

public sealed record UnitHurtEvent : SourcedUnitEvent
{
    public required int Value { get; init; }
}

public sealed record UnitAttackChangeEvent : SourcedUnitEvent
{
    public required int Value { get; init; }
}

public sealed record UnitHealthChangeEvent : SourcedUnitEvent
{
    public required int Value { get; init; }
}

public sealed record UnitDeathEvent : UnitEvent;

public sealed record UnitKillEvent : SourcedUnitEvent;
