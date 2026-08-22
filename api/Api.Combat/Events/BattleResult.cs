namespace Api.Combat.Events;

public record BattleResult
{
    public required BattleOutcome Outcome { get; init; }
    public required IReadOnlyList<BattleEvent> Events { get; init; }
}
