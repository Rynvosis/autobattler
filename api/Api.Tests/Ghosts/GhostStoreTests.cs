using Api.Combat.Units;
using Api.Ghosts;
using Api.Teams;

namespace Api.Tests.Ghosts;

public sealed class GhostStoreTests : ApiTests
{
    private static readonly DateTimeOffset ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1787769654);

    private GhostStore Ghosts => Service<GhostStore>();

    [Fact]
    public async Task FindOpponentsAsync_WhenTheStageHoldsAnotherRun_ReturnsIt()
    {
        await Ghosts.PutAsync(GhostFor("mine", 3), CancellationToken.None);
        await Ghosts.PutAsync(GhostFor("theirs", 3), CancellationToken.None);

        IReadOnlyList<Ghost> opponents = await Ghosts.FindOpponentsAsync(3, "mine", CancellationToken.None);

        Assert.Equal("theirs", Assert.Single(opponents).RunId);
    }

    [Fact]
    public async Task FindOpponentsAsync_WhenTheStageHoldsOnlyTheRunItself_ReturnsNothing()
    {
        await Ghosts.PutAsync(GhostFor("mine", 3), CancellationToken.None);

        IReadOnlyList<Ghost> opponents = await Ghosts.FindOpponentsAsync(3, "mine", CancellationToken.None);

        Assert.Empty(opponents);
    }

    [Fact]
    public async Task FindOpponentsAsync_WhenTheGhostSitsOnAnotherStage_ReturnsNothing()
    {
        await Ghosts.PutAsync(GhostFor("theirs", 2), CancellationToken.None);

        IReadOnlyList<Ghost> opponents = await Ghosts.FindOpponentsAsync(3, "mine", CancellationToken.None);

        Assert.Empty(opponents);
    }

    [Fact]
    public async Task PutAsync_WhenTheSameRunFightsTheStageAgain_ReplacesItsGhost()
    {
        await Ghosts.PutAsync(GhostFor("mine", 3), CancellationToken.None);
        await Ghosts.PutAsync(GhostFor("mine", 3) with { Units = [Unit("wyrm", 9, 9)] }, CancellationToken.None);

        IReadOnlyList<Ghost> opponents = await Ghosts.FindOpponentsAsync(3, "other", CancellationToken.None);

        Ghost stored = Assert.Single(opponents);
        Assert.Equal(9, Assert.Single(stored.Units).Attack);
    }

    [Fact]
    public async Task PutAsync_WhenTheGhostIsReadBack_ReturnsTheSameUnits()
    {
        Ghost saved = GhostFor("theirs", 4);

        await Ghosts.PutAsync(saved, CancellationToken.None);

        IReadOnlyList<Ghost> opponents = await Ghosts.FindOpponentsAsync(4, "mine", CancellationToken.None);

        Ghost loaded = Assert.Single(opponents);
        Assert.Equal(saved.Units, loaded.Units);
        Assert.Equal(saved.Stage, loaded.Stage);
        Assert.Equal(saved.ExpiresAt, loaded.ExpiresAt);
    }

    private static Ghost GhostFor(string runId, int stage)
    {
        return new Ghost
        {
            Stage = stage,
            RunId = runId,
            ExpiresAt = ExpiresAt,
            Units = [Unit("golem", 5, 10)]
        };
    }

    private static TeamUnit Unit(string kind, int attack, int health)
    {
        return new TeamUnit { Kind = new Kind(kind), Attack = attack, Health = health };
    }
}