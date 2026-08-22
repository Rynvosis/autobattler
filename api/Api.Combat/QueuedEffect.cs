using Api.Combat.Events;

namespace Api.Combat;

public abstract record QueuedEffect
{
    public required Unit Source { get; init; }
    public required IReadOnlyList<Unit> Targets { get; init; }

    public abstract void Apply(IBattleContext context);
}

public sealed record Damage : QueuedEffect
{
    public required int Value { get; init; }

    public override void Apply(IBattleContext context)
    {
        foreach (Unit target in Targets.Where(t => !t.Dead))
        {
            target.Health -= Value;
            context.Emit(EventKind.OnUnitHurt, Source, target, Value);
        }
    }
}
