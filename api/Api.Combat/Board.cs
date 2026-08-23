namespace Api.Combat;

public class Board(Team player, Team ghost)
{
    public bool IsEmpty(Side side) => TeamFor(side).IsEmpty;

    public (Unit Player, Unit Ghost) Heads() => (player.Head, ghost.Head);

    public IReadOnlyList<(Unit unit, Position position)> UnitsInIterationOrder()
    {
        List<(Unit, Position)> result = [];
        int max = Math.Max(player.Count, ghost.Count);

        for (int i = 0; i < max; i++)
        {
            if (i < player.Count) result.Add((player.Units[i], new Position(Side.Player, i)));
            if (i < ghost.Count) result.Add((ghost.Units[i], new Position(Side.Ghost, i)));
        }

        return result;
    }

    public void Remove(Unit unit)
    {
        if (player.Remove(unit) || ghost.Remove(unit)) return;

        throw new InvalidOperationException($"Unit {unit.Id} is not on the board");
    }

    private Team TeamFor(Side side) => side == Side.Player ? player : ghost;
}
