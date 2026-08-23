using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;
using Api.Combat.Events;

namespace Api.Combat.Tests;

public class TriggerTests
{
    private static BattleEvent Hurt(Board board, int targetId) =>
        new UnitHurtEvent { Source = Boards.Find(board, 1), Target = Boards.Find(board, targetId), Value = 1 };

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
                Scopes = [new HeadScope { Side = ScopeSide.Ally, Range = ScopeRange.From(0) }]
            },
            0, board => Hurt(board, 2), true
        },
        {
            new UnitTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0,
            board => new UnitAttackEvent { Source = Boards.Find(board, 1), Target = Boards.Find(board, 0), Value = 1 },
            false
        },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Matches_ReturnsExpected(Trigger trigger, int ownerId, Func<Board, BattleEvent> makeEvent, bool expected)
    {
        Board board = Boards.ThreeVersusThree();
        TriggerContext context = new(board, Boards.Find(board, ownerId));

        bool matched = trigger.Matches(makeEvent(board), context);

        Assert.Equal(expected, matched);
    }
}
