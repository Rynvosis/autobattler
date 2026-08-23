namespace Api.Combat.Abilities.Scopes;

public static class Walk
{
    public static IReadOnlyList<Unit> Ahead(TriggerContext context, Unit anchor)
    {
        (IReadOnlyList<Unit> side, int slot) = SideOf(context, anchor);

        return [.. side.Where(unit => context.Board.PositionOf(unit).Slot < slot).Reverse()];
    }

    public static IReadOnlyList<Unit> Behind(TriggerContext context, Unit anchor)
    {
        (IReadOnlyList<Unit> side, int slot) = SideOf(context, anchor);

        return [.. side.Where(unit => context.Board.PositionOf(unit).Slot > slot)];
    }

    private static (IReadOnlyList<Unit> Side, int Slot) SideOf(TriggerContext context, Unit anchor)
    {
        Position position = context.Board.PositionOf(anchor);

        return (context.Visible(position.Side), position.Slot);
    }
}

public static class ScopeSideExtensions
{
    public static Side SideIn(this ScopeSide scopeSide, TriggerContext context)
    {
        Side ownerSide = context.Board.PositionOf(context.Owner).Side;

        return scopeSide == ScopeSide.Ally ? ownerSide : ownerSide.Opposite();
    }
}
