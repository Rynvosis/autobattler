namespace Api.Combat.Abilities.Scopes;

public interface IScope<in TContext> where TContext : TriggerContext
{
    IReadOnlyList<Unit> Resolve(TContext context);
}

public interface ITriggerScope : IScope<TriggerContext>;

public interface IEffectScope : IScope<EffectContext>;

public interface IEffectAnchor : IEffectScope;

public interface IAnyScope : ITriggerScope, IEffectScope
{
    IReadOnlyList<Unit> IScope<EffectContext>.Resolve(EffectContext context) =>
        Resolve((TriggerContext)context);
}

public sealed record SelfScope : IAnyScope, IEffectAnchor
{
    public IReadOnlyList<Unit> Resolve(TriggerContext context) => [context.Owner];
}

public sealed record AbsoluteScope : IAnyScope
{
    public required ScopeSide Side { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(TriggerContext context)
    {
        Side ownerSide = context.Board.PositionOf(context.Owner).Side;
        Side targetSide = Side == ScopeSide.Ally ? ownerSide : ownerSide.Opposite();

        return Range.Slice(context.Board.Units(targetSide));
    }
}

public sealed record TriggerRelativeScope : ITriggerScope
{
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(TriggerContext context)
    {
        Position owner = context.Board.PositionOf(context.Owner);

        return Range.Slice(context.Board.Units(owner.Side), owner.Slot);
    }
}

public sealed record EffectRelativeScope : IEffectScope
{
    public required IEffectAnchor Anchor { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(EffectContext context)
    {
        if (Anchor.Resolve(context) is not [var anchorUnit]) return [];

        Position anchor = context.Board.PositionOf(anchorUnit);

        return Range.Slice(context.Board.Units(anchor.Side), anchor.Slot);
    }
}

public sealed record EventSourceScope : IEffectAnchor
{
    public IReadOnlyList<Unit> Resolve(EffectContext context) => context.EventSource is { } source ? [source] : [];
}

public sealed record EventTargetScope : IEffectAnchor
{
    public IReadOnlyList<Unit> Resolve(EffectContext context) => context.EventTarget is { } target ? [target] : [];
}

public sealed record RandomScope : IEffectScope
{
    public required int Count { get; init; }
    public required IReadOnlyList<IEffectScope> Scopes { get; init; }

    public IReadOnlyList<Unit> Resolve(EffectContext context) => throw new NotImplementedException("TODO: needs rng seed in context");
}