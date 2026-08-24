using Api.Combat.Abilities.Scopes;
using Api.Combat.Events;

namespace Api.Combat.Abilities;

public abstract record Trigger<TEvent> where TEvent : BattleEvent
{
    public abstract bool Matches(TEvent battleEvent, Context context);
}

public sealed record RoundTrigger<TEvent> : Trigger<TEvent> where TEvent : RoundEvent
{
    public override bool Matches(TEvent battleEvent, Context context) => true;
}

public sealed record UnitTrigger<TEvent> : Trigger<TEvent> where TEvent : UnitEvent
{
    public required IParticipant<TEvent> Participant { get; init; }
    public required IReadOnlyList<Any<BattleEvent>> Scopes { get; init; }

    public override bool Matches(TEvent battleEvent, Context context)
    {
        Unit participant = Participant.Of(battleEvent);

        return Scopes.Any(scope => scope.Contains(context, battleEvent, participant));
    }
}
