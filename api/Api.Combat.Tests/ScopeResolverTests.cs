using Api.Combat.Abilities.Scopes;

namespace Api.Combat.Tests;

public class ScopeResolverTests
{
    private static Board ThreeVersusThree() =>
        new(
            new Team([Unit(0), Unit(2), Unit(4)]),
            new Team([Unit(1), Unit(3), Unit(5)]));

    private static Unit Unit(int id) => new() { Id = id, Attack = 1, MaxHealth = 1 };

    private static Unit Find(Board board, int id) =>
        board.UnitsInIterationOrder().First(entry => entry.unit.Id == id).unit;

    private static Unit? FindOrNull(Board board, int? id) => id is { } value ? Find(board, value) : null;

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
        Board board = ThreeVersusThree();
        TriggerContext context = new(board, Find(board, ownerId));

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
        Board board = ThreeVersusThree();
        EffectContext context = new(
            board,
            Find(board, ownerId),
            FindOrNull(board, sourceId),
            FindOrNull(board, targetId));

        IReadOnlyList<Unit> resolved = scope.Resolve(context);

        Assert.Equal(expected, resolved.Select(unit => unit.Id));
    }
}
