using Api.Combat.Events;

namespace Api.Combat.Effects;

public interface IResolutionContext
{
    void Emit(EventKind kind, Unit? source = null, Unit? target = null, int? value = null);
}
