using Api.Combat.Battlefield;
using Api.Combat.Events;

namespace Api.Combat.Scopes;

public interface IParticipant<in TEvent> where TEvent : UnitEvent
{
    Unit Of(TEvent battleEvent);
}

public sealed record EventTarget : IParticipant<UnitEvent>, IRelation<UnitEvent>
{
    public Unit Of(UnitEvent battleEvent)
    {
        return battleEvent.Target;
    }

    public IEnumerable<Unit> Of(Context context, UnitEvent battleEvent)
    {
        return [battleEvent.Target];
    }
}

public sealed record EventSource : IParticipant<SourcedUnitEvent>, IRelation<SourcedUnitEvent>
{
    public Unit Of(SourcedUnitEvent battleEvent)
    {
        return battleEvent.Source;
    }

    public IEnumerable<Unit> Of(Context context, SourcedUnitEvent battleEvent)
    {
        return [battleEvent.Source];
    }
}
