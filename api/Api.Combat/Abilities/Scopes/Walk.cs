namespace Api.Combat.Abilities.Scopes;

public static class Walk
{
    public static IReadOnlyList<Unit> Ahead(Context context, Unit anchor)
    {
        (IReadOnlyList<Unit> side, int slot) = SideOf(context, anchor);

        return [.. side.Take(slot).Reverse()];
    }

    public static IReadOnlyList<Unit> Behind(Context context, Unit anchor)
    {
        (IReadOnlyList<Unit> side, int slot) = SideOf(context, anchor);

        return [.. side.Skip(slot + 1)];
    }

    public static IReadOnlyList<Unit> LivingAhead(Context context, Unit anchor)
    {
        return [.. Ahead(context, anchor).Where(unit => !unit.Dead)];
    }

    public static IReadOnlyList<Unit> LivingBehind(Context context, Unit anchor)
    {
        return [.. Behind(context, anchor).Where(unit => !unit.Dead)];
    }

    private static (IReadOnlyList<Unit> Side, int Slot) SideOf(Context context, Unit anchor)
    {
        Position position = context.Board.PositionOf(anchor);

        return (context.Units(position.Side), position.Slot);
    }
}

public static class ScopeSideExtensions
{
    public static Side SideIn(this ScopeSide scopeSide, Context context)
    {
        Side ownerSide = context.Board.PositionOf(context.Owner).Side;

        return scopeSide == ScopeSide.Ally ? ownerSide : ownerSide.Opposite();
    }
}
