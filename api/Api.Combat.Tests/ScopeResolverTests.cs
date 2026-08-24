using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;

namespace Api.Combat.Tests;

public class ScopeResolverTests
{
    public static TheoryData<ITriggerScope, int, int[]> TriggerCases => new()
    {
        { new SelfScope(), 0, [0] },
        { new HeadScope { Side = ScopeSide.Enemy, Range = ScopeRange.At(0) }, 0, [1] },
        { new TailScope { Side = ScopeSide.Ally, Range = ScopeRange.At(0) }, 0, [4] },
        { new TriggerBehindScope { Range = ScopeRange.At(0) }, 0, [2] },
        { new TriggerAheadScope { Range = ScopeRange.At(0) }, 4, [2] },
    };

    public static TheoryData<IEffectScope<UnitHurtEvent>, int, int, int, int[]> EffectCases => new()
    {
        { new SelfScope(), 0, 1, 3, [0] },
        { Scoped(new EventSource()), 0, 3, 1, [3] },
        { Scoped(new EventTarget()), 0, 1, 4, [4] },
        {
            new EffectBehindScope<UnitHurtEvent> { Anchor = new SelfScope(), Range = ScopeRange.At(0) },
            0, 1, 3, [2]
        },
        {
            new EffectBehindScope<UnitHurtEvent> { Anchor = Scoped(new EventSource()), Range = ScopeRange.At(0) },
            0, 3, 1, [5]
        },
        {
            new EffectAheadScope<UnitHurtEvent> { Anchor = Scoped(new EventTarget()), Range = ScopeRange.At(0) },
            0, 1, 5, [3]
        }
    };

    private static ParticipantScope<UnitHurtEvent> Scoped(IParticipant<UnitHurtEvent> participant)
    {
        return new ParticipantScope<UnitHurtEvent> { Participant = participant };
    }

    [Theory]
    [MemberData(nameof(TriggerCases))]
    public void ResolveInTriggerPosition_ReturnsExpectedUnits(ITriggerScope scope, int ownerId, int[] expected)
    {
        Board board = Boards.ThreeVersusThree();

        IReadOnlyList<Unit> resolved = scope.Resolve(new Context(board, Boards.Find(board, ownerId)));

        Assert.Equal(expected, resolved.Select(unit => unit.Id));
    }

    [Theory]
    [MemberData(nameof(EffectCases))]
    public void ResolveInEffectPosition_ReturnsExpectedUnits(
        IEffectScope<UnitHurtEvent> scope,
        int ownerId,
        int sourceId,
        int targetId,
        int[] expected)
    {
        Board board = Boards.ThreeVersusThree();

        IReadOnlyList<Unit> resolved =
            scope.Resolve(new Context(board, Boards.Find(board, ownerId)), Boards.HurtEvent(board, sourceId, targetId));

        Assert.Equal(expected, resolved.Select(unit => unit.Id));
    }

    [Fact]
    public void ResolveInEffectPosition_CorpseInThePath_SkipsToTheNextLivingUnit()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 2).Dead = true;

        EffectAheadScope<UnitHurtEvent> scope = new() { Anchor = new SelfScope(), Range = ScopeRange.At(0) };

        IReadOnlyList<Unit> resolved =
            scope.Resolve(new Context(board, Boards.Find(board, 4)), Boards.HurtEvent(board, 1, 3));

        Assert.Equal([0], resolved.Select(unit => unit.Id));
    }

    [Fact]
    public void ResolveInEffectPosition_DeadAnchor_StillWalksFromItsSlot()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 3).Dead = true;

        EffectBehindScope<UnitHurtEvent> scope =
            new() { Anchor = Scoped(new EventSource()), Range = ScopeRange.At(0) };

        IReadOnlyList<Unit> resolved =
            scope.Resolve(new Context(board, Boards.Find(board, 0)), Boards.HurtEvent(board, 3, 1));

        Assert.Equal([5], resolved.Select(unit => unit.Id));
    }
}
