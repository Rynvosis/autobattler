using Api.Combat.Units;
using Api.Runs;
using Api.Teams;

namespace Api.Tests.Runs;

public sealed class RunStoreTests(ApiFixture fixture) : ApiTests(fixture)
{
    private RunStore Runs => Service<RunStore>();

    [Fact]
    public async Task CreateAsync_WhenTheRunIsReadBack_ReturnsTheSameValues()
    {
        Run saved = await Runs.CreateAsync(NewRun(), CancellationToken.None);

        Run? loaded = await Runs.GetAsync(saved.RunId, CancellationToken.None);

        Assert.NotNull(loaded);
        TeamUnit[] noUnits = [];
        Assert.Equal(saved.Units, loaded.Units);
        Assert.Equal(saved with { Units = noUnits }, loaded with { Units = noUnits });
    }

    [Fact]
    public async Task GetAsync_WhenTheRunWasNeverWritten_ReturnsNull()
    {
        Run? run = await Runs.GetAsync("missing", CancellationToken.None);
        Assert.Null(run);
    }

    [Fact]
    public async Task UpdateAsync_WhenTheRunIsCurrent_StoresTheNextVersion()
    {
        Run created = await Runs.CreateAsync(NewRun(), CancellationToken.None);

        Run updated = await Runs.UpdateAsync(created, CancellationToken.None);

        Run? loaded = await Runs.GetAsync(created.RunId, CancellationToken.None);

        Assert.Equal(created.Version + 1, updated.Version);
        Assert.Equal(updated.Version, loaded?.Version);
    }

    [Fact]
    public async Task UpdateAsync_WhenTheStoredVersionMovedOn_ThrowsWithTheStoredRun()
    {
        Run created = await Runs.CreateAsync(NewRun(), CancellationToken.None);
        await Runs.UpdateAsync(created, CancellationToken.None);

        RunConflictException conflict =
            await Assert.ThrowsAsync<RunConflictException>(() => Runs.UpdateAsync(created, CancellationToken.None));

        Assert.Equal(2, conflict.Stored.Version);
    }

    private static Run NewRun()
    {
        return new Run
        {
            RunId = "test-run-123",
            Victories = 2,
            Gold = 10,
            Stage = 1,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1787769654),
            Units =
            [
                new TeamUnit { Kind = new Kind("goblin"), Attack = 3, Health = 5 },
                new TeamUnit { Kind = new Kind("goblin"), Attack = 7, Health = 2 }
            ]
        };
    }
}