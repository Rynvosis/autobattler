using Api.Combat.Events;

namespace Api.Combat.Abilities;

public interface IParticipant<in TEvent> where TEvent : UnitEvent
{
    Unit Of(TEvent battleEvent);
}

public sealed record EventTarget : IParticipant<UnitEvent>
{
    public Unit Of(UnitEvent battleEvent)
    {
        return battleEvent.Target;
    }
}

public sealed record EventSource : IParticipant<SourcedUnitEvent>
{
    public Unit Of(SourcedUnitEvent battleEvent)
    {
        return battleEvent.Source;
    }
}
