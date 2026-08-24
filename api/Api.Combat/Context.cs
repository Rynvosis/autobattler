namespace Api.Combat;

public record Context(Board Board, Unit Owner)
{
    public IReadOnlyList<Unit> Units(Side side)
    {
        return Board.Units(side);
    }

    public IReadOnlyList<Unit> LivingUnits(Side side)
    {
        return [.. Board.Units(side).Where(unit => !unit.Dead)];
    }
}
