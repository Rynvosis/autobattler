using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;
using Api.Combat.Events;

namespace Api.Combat.Tests;

public class TriggerTests
{
    private static Board ThreeVersusThree() =>
        new(
            new Team([Unit(0), Unit(2), Unit(4)]),
            new Team([Unit(1), Unit(3), Unit(5)]));

    private static Unit Unit(int id) => new() { Id = id, Attack = 1, MaxHealth = 1 };

    private static Unit Find(Board board, int id) =>
        board.UnitsInIterationOrder().First(entry => entry.unit.Id == id).unit;

    private static BattleEvent Hurt(Board board, int targetId) =>
        new UnitHurtEvent { Source = Find(board, 1), Target = Find(board, targetId), Value = 1 };

    public static TheoryData<Trigger, int, Func<Board, BattleEvent>, bool> Cases => new()
    {
        { new RoundTrigger<StartEvent>(), 0, _ => new StartEvent(), true },
        { new RoundTrigger<StartEvent>(), 0, board => Hurt(board, 0), false },
        {
            new UnitTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0, board => Hurt(board, 0), true
        },
        {
            new UnitTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0, board => Hurt(board, 2), false
        },
        {
            new UnitTrigger<UnitHurtEvent>
            {
                Scopes = [new AbsoluteScope { Side = ScopeSide.Ally, Range = ScopeRange.From(0) }]
            },
            0, board => Hurt(board, 2), true
        },
        {
            new UnitTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0,
            board => new UnitAttackEvent { Source = Find(board, 1), Target = Find(board, 0), Value = 1 },
            false
        },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Matches_ReturnsExpected(Trigger trigger, int ownerId, Func<Board, BattleEvent> makeEvent, bool expected)
    {
        Board board = ThreeVersusThree();
        TriggerContext context = new(board, Find(board, ownerId));

        bool matched = trigger.Matches(makeEvent(board), context);

        Assert.Equal(expected, matched);
    }
}
