using Api.Combat.Abilities;

namespace Api.Combat.Tests;

public static class Boards
{
    public static Board ThreeVersusThree() =>
        new(
            new Team([Unit(0), Unit(2), Unit(4)]),
            new Team([Unit(1), Unit(3), Unit(5)]));

    public static Unit Unit(int id, Ability? ability = null) =>
        new() { Id = id, Attack = 1, MaxHealth = 1, Ability = ability };

    public static Unit Find(Board board, int id) =>
        board.UnitsInIterationOrder().First(entry => entry.unit.Id == id).unit;

    public static Unit? FindOrNull(Board board, int? id) => id is { } value ? Find(board, value) : null;
}
