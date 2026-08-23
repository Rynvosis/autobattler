using Api.Combat.Events;

namespace Api.Combat.Effects;

public interface IResolutionContext
{
    void Emit(BattleEvent battleEvent);
}
