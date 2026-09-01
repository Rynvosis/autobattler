using Api.Combat.Battlefield;
using Api.Combat.Events;

namespace Api.Combat.Scopes;

public sealed record One<TEvent> where TEvent : BattleEvent
{
    public required IRelation<TEvent> Relation { get; init; }
    public int Index { get; init; }

    public static One<TEvent> Of(IRelation<TEvent> relation) => new() { Relation = relation };

    public Unit? Of(Context context, TEvent battleEvent) =>
        Relation.Of(context, battleEvent).Skip(Index).FirstOrDefault();
}

public sealed record Any<TEvent> where TEvent : BattleEvent
{
    public required IRelation<TEvent> Relation { get; init; }
    public ScopeRange Range { get; init; } = ScopeRange.From(0);

    public static Any<TEvent> Of(IRelation<TEvent> relation) => new() { Relation = relation };

    public bool Contains(Context context, TEvent battleEvent, Unit unit) =>
        Range.Slice([.. Relation.Of(context, battleEvent)]).Contains(unit);
}

public sealed record Every<TEvent> where TEvent : BattleEvent
{
    public required IRelation<TEvent> Relation { get; init; }
    public ScopeRange Range { get; init; } = ScopeRange.From(0);

    public static Every<TEvent> Of(IRelation<TEvent> relation) => new() { Relation = relation };

    // `reaches` is applied before the range is sliced, so "the enemy at 0" means the first one
    // the effect can actually reach.
    public IReadOnlyList<Unit> Of(Context context, TEvent battleEvent, Func<Unit, bool> reaches) =>
        Range.Slice([.. Relation.Of(context, battleEvent).Where(reaches)]);
}
