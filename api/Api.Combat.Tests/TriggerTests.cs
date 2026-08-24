using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;

namespace Api.Combat.Tests;

public class TriggerTests
{
    public static TheoryData<IParticipant<UnitHurtEvent>, IRelation<BattleEvent>, int, int, int, bool> Cases => new()
    {
        { new EventTarget(), new Self(), 0, 1, 0, true },
        { new EventTarget(), new Self(), 0, 1, 2, false },
        { new EventTarget(), new FromHead { Side = ScopeSide.Ally }, 0, 1, 2, true },
        { new EventSource(), new Self(), 0, 0, 1, true },
        { new EventSource(), new Self(), 0, 1, 0, false },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Matches_ReturnsExpected(
        IParticipant<UnitHurtEvent> participant,
        IRelation<BattleEvent> relation,
        int ownerId,
        int sourceId,
        int targetId,
        bool expected)
    {
        Board board = Boards.ThreeVersusThree();

        UnitTrigger<UnitHurtEvent> trigger = new()
        {
            Participant = participant,
            Scopes = [Any<BattleEvent>.Of(relation)]
        };

        bool matched = trigger.Matches(
            Boards.HurtEvent(board, sourceId, targetId),
            new Context(board, Boards.Find(board, ownerId)));

        Assert.Equal(expected, matched);
    }

    [Fact]
    public void RoundTrigger_MatchesWithoutAScope()
    {
        Board board = Boards.ThreeVersusThree();

        Assert.True(new RoundTrigger<StartEvent>().Matches(new StartEvent(), new Context(board, Boards.Find(board, 0))));
    }
}
