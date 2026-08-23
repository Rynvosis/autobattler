using Api.Combat.Abilities.Scopes;

namespace Api.Combat.Tests;

public class ScopeResolverTests
{
    public static TheoryData<ITriggerScope, int, int[]> TriggerCases => new()
    {
        { new SelfScope(), 0, [0] },
        { new AbsoluteScope { Side = ScopeSide.Enemy, Range = ScopeRange.At(0) }, 0, [1] },
        { new TriggerRelativeScope { Range = ScopeRange.At(1) }, 0, [2] },
    };

    [Theory]
    [MemberData(nameof(TriggerCases))]
    public void ResolveInTriggerContext_ReturnsExpectedUnits(ITriggerScope scope, int ownerId, int[] expected)
    {
        Board board = Boards.ThreeVersusThree();
        TriggerContext context = new(board, Boards.Find(board, ownerId));

        IReadOnlyList<Unit> resolved = scope.Resolve(context);

        Assert.Equal(expected, resolved.Select(unit => unit.Id));
    }

    public static TheoryData<IEffectScope, int, int?, int?, int[]> EffectCases => new()
    {
        { new SelfScope(), 0, null, null, [0] },
        { new EventSourceScope(), 0, 3, null, [3] },
        { new EventSourceScope(), 0, null, null, [] },
        { new EventTargetScope(), 0, null, 4, [4] },
        { new EffectRelativeScope { Anchor = new SelfScope(), Range = ScopeRange.At(1) }, 0, null, null, [2] },
        { new EffectRelativeScope { Anchor = new EventSourceScope(), Range = ScopeRange.At(1) }, 0, 3, null, [5] },
        { new EffectRelativeScope { Anchor = new EventTargetScope(), Range = ScopeRange.At(-1) }, 0, null, 5, [3] },
    };

    [Theory]
    [MemberData(nameof(EffectCases))]
    public void ResolveInEffectContext_ReturnsExpectedUnits(
        IEffectScope scope,
        int ownerId,
        int? sourceId,
        int? targetId,
        int[] expected)
    {
        Board board = Boards.ThreeVersusThree();
        EffectContext context = new(
            board,
            Boards.Find(board, ownerId),
            Boards.FindOrNull(board, sourceId),
            Boards.FindOrNull(board, targetId));

        IReadOnlyList<Unit> resolved = scope.Resolve(context);

        Assert.Equal(expected, resolved.Select(unit => unit.Id));
    }
}
