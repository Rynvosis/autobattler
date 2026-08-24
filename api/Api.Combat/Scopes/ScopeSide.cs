using Api.Combat.Battlefield;

namespace Api.Combat.Scopes;

public enum ScopeSide
{
    Ally,
    Enemy,
}

public static class ScopeSideExtensions
{
    public static Side SideIn(this ScopeSide scopeSide, Context context)
    {
        Side ownerSide = context.Board.PositionOf(context.Owner).Side;

        return scopeSide == ScopeSide.Ally ? ownerSide : ownerSide.Opposite();
    }
}
