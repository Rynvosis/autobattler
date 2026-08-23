namespace Api.Combat;

public enum Side
{
    Player,
    Ghost,
}

public static class SideExtensions
{
    public static Side Opposite(this Side side) => side == Side.Player ? Side.Ghost : Side.Player;
}
