using Api.Combat.Abilities;
using Api.Combat.Scopes;

namespace Api.Combat.Tests;

public class ScopeTests
{
    private static One<UnitHurtEvent> Anchor(IRelation<UnitHurtEvent> relation) => One<UnitHurtEvent>.Of(relation);

    public static TheoryData<IRelation<UnitHurtEvent>, ScopeRange, int, int, int, int[]> EveryCases => new()
    {
        { new Self(), ScopeRange.From(0), 0, 1, 3, [0] },
        { new FromHead { Side = ScopeSide.Enemy }, ScopeRange.At(0), 0, 1, 3, [1] },
        { new FromTail { Side = ScopeSide.Ally }, ScopeRange.At(0), 0, 1, 3, [4] },
        { new FromHead { Side = ScopeSide.Ally }, ScopeRange.From(0), 0, 1, 3, [0, 2, 4] },
        { new EventUnit<UnitHurtEvent> { Participant = new EventSource() }, ScopeRange.From(0), 0, 3, 1, [3] },
        { new EventUnit<UnitHurtEvent> { Participant = new EventTarget() }, ScopeRange.From(0), 0, 1, 4, [4] },
        { new Behind<UnitHurtEvent> { Anchor = Anchor(new Self()) }, ScopeRange.At(0), 0, 1, 3, [2] },
        { new Ahead<UnitHurtEvent> { Anchor = Anchor(new Self()) }, ScopeRange.At(0), 4, 1, 3, [2] },
        {
            new Behind<UnitHurtEvent> { Anchor = Anchor(new EventUnit<UnitHurtEvent> { Participant = new EventSource() }) },
            ScopeRange.At(0), 0, 3, 1, [5]
        },
    };

    [Theory]
    [MemberData(nameof(EveryCases))]
    public void Every_ReturnsExpectedUnits(
        IRelation<UnitHurtEvent> relation,
        ScopeRange range,
        int ownerId,
        int sourceId,
        int targetId,
        int[] expected)
    {
        Board board = Boards.ThreeVersusThree();
        Every<UnitHurtEvent> scope = new() { Relation = relation, Range = range };

        IReadOnlyList<Unit> resolved =
            scope.Of(new Context(board, Boards.Find(board, ownerId)), Boards.HurtEvent(board, sourceId, targetId));

        Assert.Equal(expected, resolved.Select(unit => unit.Id));
    }

    [Fact]
    public void Every_CorpseInThePath_SkipsToTheNextLivingUnit()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 2).Dead = true;

        Every<UnitHurtEvent> scope = new()
        {
            Relation = new Ahead<UnitHurtEvent> { Anchor = Anchor(new Self()) },
            Range = ScopeRange.At(0)
        };

        IReadOnlyList<Unit> resolved =
            scope.Of(new Context(board, Boards.Find(board, 4)), Boards.HurtEvent(board, 1, 3));

        Assert.Equal([0], resolved.Select(unit => unit.Id));
    }

    [Fact]
    public void Every_DeadUnitInRelation_IsExcluded()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 2).Dead = true;

        Every<UnitHurtEvent> scope = new() { Relation = new FromHead { Side = ScopeSide.Ally } };

        IReadOnlyList<Unit> resolved =
            scope.Of(new Context(board, Boards.Find(board, 0)), Boards.HurtEvent(board, 1, 3));

        Assert.Equal([0, 4], resolved.Select(unit => unit.Id));
    }

    [Fact]
    public void One_DeadAnchor_StillResolves()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 3).Dead = true;

        One<UnitHurtEvent> anchor = Anchor(new EventUnit<UnitHurtEvent> { Participant = new EventSource() });

        Unit? resolved = anchor.Of(new Context(board, Boards.Find(board, 0)), Boards.HurtEvent(board, 3, 1));

        Assert.Equal(3, resolved?.Id);
    }

    [Fact]
    public void Any_SeesTheDead()
    {
        Board board = Boards.ThreeVersusThree();
        Unit dead = Boards.Find(board, 2);
        dead.Dead = true;

        Any<UnitHurtEvent> scope = new() { Relation = new FromHead { Side = ScopeSide.Ally } };

        Assert.True(scope.Contains(new Context(board, Boards.Find(board, 0)), Boards.HurtEvent(board, 1, 3), dead));
    }
}
