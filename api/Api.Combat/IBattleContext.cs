using Api.Combat.Events;

namespace Api.Combat;

public interface IBattleContext
{
    void Emit(EventKind kind, Unit? source = null, Unit? target = null, int? value = null);
}
