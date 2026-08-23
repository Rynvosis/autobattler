using Api.Combat.Events;

namespace Api.Combat.Effects;

public abstract record Effect
{
    public abstract void Apply(IResolutionContext context, Unit source, IReadOnlyList<Unit> targets);
}

public sealed record Damage : Effect
{
    public required int Value { get; init; }

    public override void Apply(IResolutionContext context, Unit source, IReadOnlyList<Unit> targets)
    {
        foreach (Unit target in targets.Where(t => !t.Dead))
        {
            target.Health -= Value;
            if (Value > 0)
            {
                context.Emit(new UnitHurtEvent { Source = source, Target = target, Value = Value });
            }
        }
    }
}
