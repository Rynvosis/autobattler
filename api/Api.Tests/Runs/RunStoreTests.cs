using Api.Combat.Units;
using Api.Runs;
using Api.Teams;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.DynamoDb;

namespace Api.Tests.Runs;

public sealed class RunStoreTests : IAsyncLifetime
{
    private readonly DynamoDbContainer _container = new DynamoDbBuilder("amazon/dynamodb-local:latest").Build();

    private WebApplicationFactory<Program> _api = null!;

    private RunStore _store = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _api = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
        {
            host.UseSetting("AWS:Region", "eu-west-2");
            host.UseSetting("DynamoDB:ServiceUrl", _container.GetConnectionString());
        });

        _store = _api.Services.GetRequiredService<RunStore>();
    }

    public async Task DisposeAsync()
    {
        await _api.DisposeAsync();
        await _container.DisposeAsync();
    }

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
        await _store.PutAsync(saved, CancellationToken.None);

        Run? loaded = await _store.GetAsync(saved.RunId, CancellationToken.None);

        Assert.NotNull(loaded);
        TeamUnit[] noUnits = [];
        Assert.Equal(saved.Units, loaded.Units);
        Assert.Equal(saved with { Units = noUnits }, loaded with { Units = noUnits });
    }

    [Fact]
    public async Task GetAsync_WhenTheRunWasNeverWritten_ReturnsNull()
    {
        Run? run = await _store.GetAsync("missing", CancellationToken.None);
        Assert.Null(run);
    }
}