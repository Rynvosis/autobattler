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

    public bool Finished => Stage > Economy.TotalStages;

    public Run AfterBattle(BattleOutcome outcome)
    {
        return this with
        {
            Victories = outcome == BattleOutcome.Win ? Victories + 1 : Victories,
            Gold = Gold + Economy.GoldFor(outcome),
            Stage = Stage + 1
        };
    }
}