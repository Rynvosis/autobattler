using Api.Combat.Events;

namespace Api.Combat.Battles;

public record BattleResult
{
    public required BattleOutcome Outcome { get; init; }
    public required IReadOnlyList<BattleEvent> Events { get; init; }
}
