using Api.Combat.Battles;
using Api.Teams;

namespace Api.Runs;

public sealed record Run
{
    public required string RunId { get; init; }
    public int Version { get; init; }
    public required int Victories { get; init; }
    public required int Gold { get; init; }
    public required int Stage { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IReadOnlyList<TeamUnit> Units { get; init; }

    // A bought slot is nulled rather than compacted, so a slot index means the same thing to the
    // client between rendering the offer and clicking it.
    public required IReadOnlyList<TeamUnit?> Shop { get; init; }

    // Duplicating is allowed once per stage.
    public bool Duplicated { get; init; }
    public int UpgradeCredits { get; init; }

    public bool Finished => Stage > Economy.TotalStages;

    public Run AfterBattle(BattleOutcome outcome, IReadOnlyList<TeamUnit?> shop)
    {
        return this with
        {
            Victories = outcome == BattleOutcome.Win ? Victories + 1 : Victories,
            Gold = Gold + Economy.GoldFor(outcome),
            Stage = Stage + 1,
            Shop = shop,
            Duplicated = false
        };
    }
}