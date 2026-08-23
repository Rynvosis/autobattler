using Api.Combat.Events;
using Api.Combat.Scheduling;

namespace Api.Combat.Abilities;

public abstract record Effect
{
    public abstract void Apply(IBattleContext context, Unit source, IReadOnlyList<Unit> targets);
}

public sealed record Damage : Effect
{
    public required int Value { get; init; }

    public override void Apply(IBattleContext context, Unit source, IReadOnlyList<Unit> targets)
    {
        foreach (Unit target in targets.Where(t => !t.Dead))
        {
            target.Health -= Value;
            if (Value > 0)
            {
                context.Emit(EventKind.OnUnitHurt, source, target, Value);
            }
        }
    }
}
