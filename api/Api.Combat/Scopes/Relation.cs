using Api.Combat.Battlefield;
using Api.Combat.Events;

namespace Api.Combat.Scopes;

public interface IRelation<in TEvent> where TEvent : BattleEvent
{
    IEnumerable<Unit> Of(Context context, TEvent battleEvent);
}

public sealed record Self : IRelation<BattleEvent>
{
    public IEnumerable<Unit> Of(Context context, BattleEvent battleEvent) => [context.Owner];
}

public sealed record FromHead : IRelation<BattleEvent>
{
    public required ScopeSide Side { get; init; }

    public IEnumerable<Unit> Of(Context context, BattleEvent battleEvent) => context.Units(Side.SideIn(context));
}

public sealed record FromTail : IRelation<BattleEvent>
{
    public required ScopeSide Side { get; init; }

    public IEnumerable<Unit> Of(Context context, BattleEvent battleEvent) =>
        context.Units(Side.SideIn(context)).Reverse();
}

public sealed record Ahead<TEvent> : IRelation<TEvent> where TEvent : BattleEvent
{
    public required One<TEvent> Anchor { get; init; }

    public IEnumerable<Unit> Of(Context context, TEvent battleEvent) =>
        Anchor.Of(context, battleEvent) is { } anchor ? Walk.Ahead(context, anchor) : [];
}

public sealed record Behind<TEvent> : IRelation<TEvent> where TEvent : BattleEvent
{
    public required One<TEvent> Anchor { get; init; }

    public IEnumerable<Unit> Of(Context context, TEvent battleEvent) =>
        Anchor.Of(context, battleEvent) is { } anchor ? Walk.Behind(context, anchor) : [];
}

public sealed record OfKind<TEvent> : IRelation<TEvent> where TEvent : BattleEvent
{
    public required IRelation<TEvent> Relation { get; init; }
    public required Kind Kind { get; init; }

    public IEnumerable<Unit> Of(Context context, TEvent battleEvent)
    {
        return Relation.Of(context, battleEvent).Where(unit => unit.Kind == Kind);
    }
}
