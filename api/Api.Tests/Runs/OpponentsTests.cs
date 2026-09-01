using Api.Combat.Battlefield;
using Api.Combat.Units;
using Api.Ghosts;
using Api.Runs;
using Api.Teams;

namespace Api.Tests.Runs;

public sealed class OpponentsTests(ApiFixture fixture) : ApiTests(fixture)
{
    private const int MinimumCandidates = 5;

    private GhostStore Ghosts => Service<GhostStore>();

    [Fact]
    public async Task FindOrCreateTeamAsync_WhenTheStageIsEmpty_FillsIt()
    {
        await Opponents.FindOrCreateTeamAsync(Ghosts, RunAtStage(1), CancellationToken.None);

        IReadOnlyList<Ghost> stage = await StageAsync(1);

        Assert.Equal(MinimumCandidates, stage.Count);
        Assert.All(stage, ghost => Assert.StartsWith("zz-filler-1-", ghost.RunId));
    }

    [Fact]
    public async Task FindOrCreateTeamAsync_WhenCalledTwice_AddsNoFurtherFillers()
    {
        await Opponents.FindOrCreateTeamAsync(Ghosts, RunAtStage(1), CancellationToken.None);
        await Opponents.FindOrCreateTeamAsync(Ghosts, RunAtStage(1), CancellationToken.None);

        Assert.Equal(MinimumCandidates, (await StageAsync(1)).Count);
    }

    [Fact]
    public async Task FindOrCreateTeamAsync_WhenTheStageHoldsARealGhost_FillsOnlyTheRemainder()
    {
        await Ghosts.PutAsync(new Ghost
        {
            Stage = 1,
            RunId = "someone-else",
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1787769654),
            Units = [new TeamUnit { Kind = new Kind("golem"), Attack = 5, Health = 10 }]
        }, CancellationToken.None);

        await Opponents.FindOrCreateTeamAsync(Ghosts, RunAtStage(1), CancellationToken.None);

        IReadOnlyList<Ghost> stage = await StageAsync(1);

        Assert.Equal(MinimumCandidates, stage.Count);
        Assert.Equal(MinimumCandidates - 1, stage.Count(ghost => ghost.RunId.StartsWith("zz-filler-")));
    }

    [Fact]
    public async Task FindOrCreateTeamAsync_GivesTheOpponentUnitIdsAfterThePlayers()
    {
        Run run = RunAtStage(1);

        Team opponent = await Opponents.FindOrCreateTeamAsync(Ghosts, run, CancellationToken.None);

        Assert.NotEmpty(opponent.Units);
        Assert.All(opponent.Units, unit => Assert.True(unit.Id >= run.Units.Count));
    }

    [Fact]
    public async Task FindOrCreateTeamAsync_FillsEachStageSeparately()
    {
        await Opponents.FindOrCreateTeamAsync(Ghosts, RunAtStage(1), CancellationToken.None);
        await Opponents.FindOrCreateTeamAsync(Ghosts, RunAtStage(2), CancellationToken.None);

        Assert.All(await StageAsync(2), ghost => Assert.StartsWith("zz-filler-2-", ghost.RunId));
    }

    private Task<IReadOnlyList<Ghost>> StageAsync(int stage)
    {
        return Ghosts.FindOpponentsAsync(stage, "nobody", CancellationToken.None);
    }

    private static Run RunAtStage(int stage)
    {
        return new Run
        {
            RunId = "test-run",
            Victories = 0,
            Gold = Economy.StartingGold,
            Stage = stage,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1787769654),
            Units = [new TeamUnit { Kind = new Kind("golem"), Attack = 5, Health = 10 }],
            Shop = []
        };
    }
}