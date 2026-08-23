namespace Api.Combat.Effects;

public record QueuedEffect
{
    public required Effect Effect { get; init; }
    public required Unit Source { get; init; }
    public required IReadOnlyList<Unit> Targets { get; init; }

    public void Apply(IResolutionContext context) => Effect.Apply(context, Source, Targets);
}
