using Api.Combat.Events;

namespace Api.Combat.Effects;

public abstract record Effect<TEvent> where TEvent : BattleEvent
{
    public abstract IReadOnlyList<BattleEvent> Apply(Context context, TEvent battleEvent, Unit recipient);
}

public sealed record Damage<TEvent> : Effect<TEvent> where TEvent : BattleEvent
{
    public required IValue<TEvent> Value { get; init; }

    public override IReadOnlyList<BattleEvent> Apply(Context context, TEvent battleEvent, Unit recipient)
    {
        int value = Value.Resolve(context, battleEvent, recipient);

        recipient.Health -= value;

        return value > 0 ? [new UnitHurtEvent { Source = context.Owner, Target = recipient, Value = value }] : [];
    }
}

public sealed record StatChange<TEvent> : Effect<TEvent> where TEvent : BattleEvent
{
    public required IValue<TEvent> Attack { get; init; }
    public required IValue<TEvent> Health { get; init; }

    public override IReadOnlyList<BattleEvent> Apply(Context context, TEvent battleEvent, Unit recipient)
    {
        int attack = Attack.Resolve(context, battleEvent, recipient);
        int health = Health.Resolve(context, battleEvent, recipient);

        recipient.Attack += attack;
        recipient.Health += health;

        List<BattleEvent> events = [];

        if (attack != 0)
            events.Add(new UnitAttackChangeEvent { Source = context.Owner, Target = recipient, Value = attack });

        if (health != 0)
            events.Add(new UnitHealthChangeEvent { Source = context.Owner, Target = recipient, Value = health });

        return events;
    }
}
