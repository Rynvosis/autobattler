using Api.Combat.Units;
using Api.Runs;
using Api.Teams;

namespace Api.Tests.Runs;

public sealed class RunStoreTests : ApiTests
{
    private RunStore Runs => Service<RunStore>();

    [Fact]
    public async Task PutAsync_WhenTheRunIsReadBack_ReturnsTheSameValues()
    {
        Run saved = new()
        {
            RunId = "test-run-123",
            Version = 1,
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
        await Runs.PutAsync(saved, CancellationToken.None);

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
}