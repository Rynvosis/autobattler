namespace Api.Combat;

public record TriggerContext(Board Board, Unit Owner)
{
    public virtual IReadOnlyList<Unit> Visible(Side side) => Board.Units(side);
}

public record EffectContext(Board Board, Unit Owner, Unit? EventSource, Unit? EventTarget)
    : TriggerContext(Board, Owner)
{
    public override IReadOnlyList<Unit> Visible(Side side) =>
        [.. Board.Units(side).Where(unit => !unit.Dead)];
}
