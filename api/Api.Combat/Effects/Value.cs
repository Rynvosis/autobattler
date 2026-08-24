using Api.Combat.Abilities.Scopes;
using Api.Combat.Events;

namespace Api.Combat.Effects;

public interface IValue<in TEvent> where TEvent : BattleEvent
{
    int Resolve(Context context, TEvent battleEvent, Unit recipient);
}

public sealed record Literal : IValue<BattleEvent>
{
    public required int Amount { get; init; }

    public static Literal Of(int amount) => new() { Amount = amount };

    public int Resolve(Context context, BattleEvent battleEvent, Unit recipient) => Amount;
}

public sealed record RecipientStat : IValue<BattleEvent>
{
    public required Stat Stat { get; init; }

    public int Resolve(Context context, BattleEvent battleEvent, Unit recipient) => Stat.Of(recipient);
}

public sealed record UnitStat<TEvent> : IValue<TEvent> where TEvent : BattleEvent
{
    public required One<TEvent> Subject { get; init; }
    public required Stat Stat { get; init; }

    public int Resolve(Context context, TEvent battleEvent, Unit recipient) =>
        Subject.Of(context, battleEvent) is { } unit ? Stat.Of(unit) : 0;
}
