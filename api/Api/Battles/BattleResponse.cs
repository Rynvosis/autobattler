using Api.Combat.Battles;
using Api.Runs;

namespace Api.Battles;

public sealed record BattleResponse
{
    public required Run Run { get; init; }
    public required BattleRecord Battle { get; init; }

    // Part of the run's new gold, so the client can name its source without deciding for
    // itself which bounties were the player's.
    public required int BountyEarned { get; init; }
}

public sealed record BattleRecord
{
    public required BattleOutcome Outcome { get; init; }
    public required IReadOnlyList<UnitRecord> Player { get; init; }
    public required IReadOnlyList<UnitRecord> Opponent { get; init; }
    public required IReadOnlyList<EventRecord> Events { get; init; }
}
