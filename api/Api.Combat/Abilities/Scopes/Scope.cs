using Api.Combat.Events;

namespace Api.Combat.Abilities.Scopes;

public interface ITriggerScope
{
    IReadOnlyList<Unit> Resolve(Context context);
}

public interface IEffectScope<in TEvent> where TEvent : BattleEvent
{
    IReadOnlyList<Unit> Resolve(Context context, TEvent battleEvent);
}

public interface IEffectAnchor<in TEvent> : IEffectScope<TEvent> where TEvent : BattleEvent
{
    IReadOnlyList<Unit> IEffectScope<TEvent>.Resolve(Context context, TEvent battleEvent)
    {
        return [Of(context, battleEvent)];
    }

    Unit Of(Context context, TEvent battleEvent);
}

public sealed record SelfScope : ITriggerScope, IEffectAnchor<BattleEvent>
{
    public Unit Of(Context context, BattleEvent battleEvent)
    {
        return context.Owner;
    }

    public IReadOnlyList<Unit> Resolve(Context context)
    {
        return [context.Owner];
    }
}

public sealed record ParticipantScope<TEvent> : IEffectAnchor<TEvent> where TEvent : UnitEvent
{
    public required IParticipant<TEvent> Participant { get; init; }

    public Unit Of(Context context, TEvent battleEvent)
    {
        return Participant.Of(battleEvent);
    }
}

public sealed record HeadScope : ITriggerScope, IEffectScope<BattleEvent>
{
    public required ScopeSide Side { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(Context context, BattleEvent battleEvent)
    {
        return Range.Slice(context.LivingUnits(Side.SideIn(context)));
    }

    public IReadOnlyList<Unit> Resolve(Context context)
    {
        return Range.Slice(context.Units(Side.SideIn(context)));
    }
}

public sealed record TailScope : ITriggerScope, IEffectScope<BattleEvent>
{
    public required ScopeSide Side { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(Context context, BattleEvent battleEvent)
    {
        return Range.Slice([.. context.LivingUnits(Side.SideIn(context)).Reverse()]);
    }

    public IReadOnlyList<Unit> Resolve(Context context)
    {
        return Range.Slice([.. context.Units(Side.SideIn(context)).Reverse()]);
    }
}

public sealed record TriggerAheadScope : ITriggerScope
{
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(Context context)
    {
        return Range.Slice(Walk.Ahead(context, context.Owner));
    }
}

public sealed record TriggerBehindScope : ITriggerScope
{
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(Context context)
    {
        return Range.Slice(Walk.Behind(context, context.Owner));
    }
}

public sealed record EffectAheadScope<TEvent> : IEffectScope<TEvent> where TEvent : BattleEvent
{
    public required IEffectAnchor<TEvent> Anchor { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(Context context, TEvent battleEvent)
    {
        return Range.Slice(Walk.LivingAhead(context, Anchor.Of(context, battleEvent)));
    }
}

public sealed record EffectBehindScope<TEvent> : IEffectScope<TEvent> where TEvent : BattleEvent
{
    public required IEffectAnchor<TEvent> Anchor { get; init; }
    public required ScopeRange Range { get; init; }

    public IReadOnlyList<Unit> Resolve(Context context, TEvent battleEvent)
    {
        return Range.Slice(Walk.LivingBehind(context, Anchor.Of(context, battleEvent)));
    }
}

public sealed record RandomScope<TEvent> : IEffectScope<TEvent> where TEvent : BattleEvent
{
    public required int Count { get; init; }
    public required IReadOnlyList<IEffectScope<TEvent>> Scopes { get; init; }

    //todo: needs the battle rng declared and a reference passed into the context
    public IReadOnlyList<Unit> Resolve(Context context, TEvent battleEvent)
    {
        throw new NotImplementedException();
    }
}
