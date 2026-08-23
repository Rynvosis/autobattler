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

public sealed record HeadScope : IAnyScope
{
    public required ScopeSide Side { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(TriggerContext context) =>
        Range.Slice(context.Visible(Side.SideIn(context)));
}

public sealed record TailScope : IAnyScope
{
    public required ScopeSide Side { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(TriggerContext context) =>
        Range.Slice([.. context.Visible(Side.SideIn(context)).Reverse()]);
}

public sealed record TriggerAheadScope : ITriggerScope
{
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(TriggerContext context) =>
        Range.Slice(Walk.Ahead(context, context.Owner));
}

public sealed record TriggerBehindScope : ITriggerScope
{
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(TriggerContext context) =>
        Range.Slice(Walk.Behind(context, context.Owner));
}

public sealed record EffectAheadScope : IEffectScope
{
    public required IEffectAnchor Anchor { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(EffectContext context) =>
        Anchor.Resolve(context) is [var anchor] ? Range.Slice(Walk.Ahead(context, anchor)) : [];
}

public sealed record EffectBehindScope : IEffectScope
{
    public required IEffectAnchor Anchor { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(EffectContext context) =>
        Anchor.Resolve(context) is [var anchor] ? Range.Slice(Walk.Behind(context, anchor)) : [];
}

public sealed record EventSourceScope : IEffectAnchor
{
    public IReadOnlyList<Unit> Resolve(EffectContext context) =>
        context.EventSource is { } source ? [source] : [];
}

public sealed record EventTargetScope : IEffectAnchor
{
    public IReadOnlyList<Unit> Resolve(EffectContext context) =>
        context.EventTarget is { } target ? [target] : [];
}

public sealed record RandomScope : IEffectScope
{
    public required int Count { get; init; }
    public required IReadOnlyList<IEffectScope> Scopes { get; init; }

    //todo: needs the battle rng declared and a reference passed into the context
    public IReadOnlyList<Unit> Resolve(EffectContext context) => throw new NotImplementedException();
}
