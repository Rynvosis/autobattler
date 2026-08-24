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

public sealed record StatChange : Effect
{
    public required int Attack { get; init; }
    public required int Health { get; init; }

    public override void Apply(IResolutionContext context, Unit source, IReadOnlyList<Unit> targets)
    {
        foreach (Unit target in targets.Where(t => !t.Dead))
        {
            target.Attack += Attack;
            target.Health += Health;

            if (Attack != 0)
                context.Emit(new UnitAttackChangeEvent { Source = source, Target = target, Value = Attack });

            if (Health != 0)
                context.Emit(new UnitHealthChangeEvent { Source = source, Target = target, Value = Health });
        }
    }
}
