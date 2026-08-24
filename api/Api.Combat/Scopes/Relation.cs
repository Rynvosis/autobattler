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

public sealed record EventUnit<TEvent> : IRelation<TEvent> where TEvent : UnitEvent
{
    public required IParticipant<TEvent> Participant { get; init; }

    public IEnumerable<Unit> Of(Context context, TEvent battleEvent) => [Participant.Of(battleEvent)];
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
