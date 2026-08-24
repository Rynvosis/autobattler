using Api.Combat.Abilities;
using Api.Combat.Effects;
using Api.Combat.Scopes;

namespace Api.Combat.Tests;

public static class Boards
{
    public static Board ThreeVersusThree() =>
        new(
            new Team([Unit(0), Unit(2), Unit(4)]),
            new Team([Unit(1), Unit(3), Unit(5)]));

    public static Unit Unit(int id, string kind = "dummy")
    {
        return new Unit { Id = id, Kind = new Kind(kind), Attack = 1, Health = 1 };
    }

    public static Unit Find(Board board, int id) =>
        board.UnitsInIterationOrder().First(entry => entry.unit.Id == id).unit;

    public static UnitHurtEvent HurtEvent(Board board, int sourceId, int targetId) =>
        new() { Source = Find(board, sourceId), Target = Find(board, targetId), Value = 1 };

    public static Ability Retaliate() =>
        new Ability<UnitHurtEvent>
        {
            Trigger = new UnitTrigger<UnitHurtEvent>
            {
                Participant = new EventTarget(),
                Scopes = [Any<BattleEvent>.Of(new Self())]
            },
            Effects =
            [
                new ScopedEffect<UnitHurtEvent>
                {
                    Effect = new Damage<UnitHurtEvent> { Value = Literal.Of(1) },
                    Scopes = [Every<UnitHurtEvent>.Of(new EventSource())]
                }
            ]
        };
}
