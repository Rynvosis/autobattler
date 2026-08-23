namespace Api.Combat.Events;

public abstract record BattleEvent
{
    public int Tick { get; init; }
    public int Subtick { get; init; }

    public abstract EffectContext ContextFor(Board board, Unit owner);
}

public abstract record RoundEvent : BattleEvent
{
    public override EffectContext ContextFor(Board board, Unit owner) => new(board, owner, null, null);
}

public abstract record UnitEvent : BattleEvent
{
    public required Unit Target { get; init; }

    public override EffectContext ContextFor(Board board, Unit owner) => new(board, owner, null, Target);
}

public sealed record StartEvent : RoundEvent;

public sealed record UnitAttackEvent : UnitEvent
{
    public required Unit Source { get; init; }
    public required int Value { get; init; }

    public override EffectContext ContextFor(Board board, Unit owner) => new(board, owner, Source, Target);
}

public sealed record UnitHurtEvent : UnitEvent
{
    public required Unit Source { get; init; }
    public required int Value { get; init; }

    public override EffectContext ContextFor(Board board, Unit owner) => new(board, owner, Source, Target);
}

public sealed record UnitDeathEvent : UnitEvent;
