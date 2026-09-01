using Api.Combat.Battles;
using Api.Runs;

namespace Api.Battles;

public sealed record BattleResponse
{
    public required Run Run { get; init; }
    public required BattleRecord Battle { get; init; }
}

public sealed record BattleRecord
{
    public required BattleOutcome Outcome { get; init; }
    public required IReadOnlyList<UnitRecord> Player { get; init; }
    public required IReadOnlyList<UnitRecord> Opponent { get; init; }
    public required IReadOnlyList<EventRecord> Events { get; init; }
}
