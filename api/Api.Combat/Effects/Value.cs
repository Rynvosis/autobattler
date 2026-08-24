using Api.Combat.Abilities;
using Api.Combat.Events;

namespace Api.Combat.Effects;

public interface IValue<in TEvent> where TEvent : BattleEvent
{
    int Resolve(Context context, TEvent battleEvent, Unit recipient);
}

public sealed record Literal : IValue<BattleEvent>
{
    public required int Amount { get; init; }

    public int Resolve(Context context, BattleEvent battleEvent, Unit recipient)
    {
        return Amount;
    }

    public static Literal Of(int amount)
    {
        return new Literal { Amount = amount };
    }
}

public sealed record SelfStat : IValue<BattleEvent>
{
    public required Stat Stat { get; init; }

    public int Resolve(Context context, BattleEvent battleEvent, Unit recipient)
    {
        return Stat.Of(context.Owner);
    }
}

public sealed record RecipientStat : IValue<BattleEvent>
{
    public required Stat Stat { get; init; }

    public int Resolve(Context context, BattleEvent battleEvent, Unit recipient)
    {
        return Stat.Of(recipient);
    }
}

public sealed record ParticipantStat<TEvent> : IValue<TEvent> where TEvent : UnitEvent
{
    public required IParticipant<TEvent> Participant { get; init; }
    public required Stat Stat { get; init; }

    public int Resolve(Context context, TEvent battleEvent, Unit recipient)
    {
        return Stat.Of(Participant.Of(battleEvent));
    }
}
