using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Ghosts;
using Api.Runs;

namespace Api.Tests.Runs;

public sealed class RunEndpointsTests : ApiTests
{
    private RunStore Runs => Service<RunStore>();

    private GhostStore Ghosts => Service<GhostStore>();

    [Fact]
    public async Task CreateRun_StartsAtStageOneWithStartingGold()
    {
        Run run = await CreateRunAsync();

        Assert.Equal(1, run.Stage);
        Assert.Equal(0, run.Victories);
        Assert.Equal(Economy.StartingGold, run.Gold);
        Assert.NotEmpty(run.Units);
        Assert.False(run.Finished);
    }

    [Fact]
    public async Task CreateRun_WritesAUnitKindAsAString()
    {
        HttpResponseMessage response = await PostRunAsync();

        JsonElement run = await response.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement kind = run.GetProperty("units")[0].GetProperty("kind");

        Assert.Equal(JsonValueKind.String, kind.ValueKind);
    }

    [Fact]
    public async Task CreateRun_StoresTheRun()
    {
        Run created = await CreateRunAsync();

        Run? stored = await Runs.GetAsync(created.RunId, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(created.RunId, stored.RunId);
    }

    [Fact]
    public async Task FightBattle_AdvancesTheStageAndPaysGold()
    {
        Run created = await CreateRunAsync();

        HttpResponseMessage response = await Client.PostAsync($"/runs/{created.RunId}/battle", null);
        response.EnsureSuccessStatusCode();

        Run? after = await Runs.GetAsync(created.RunId, CancellationToken.None);

        Assert.NotNull(after);
        Assert.Equal(2, after.Stage);
        Assert.Equal(created.Version + 1, after.Version);
        Assert.True(after.Gold > created.Gold);
    }

    [Fact]
    public async Task FightBattle_ReturnsTheRunBothTeamsAndTypedEvents()
    {
        Run created = await CreateRunAsync();

        HttpResponseMessage response = await Client.PostAsync($"/runs/{created.RunId}/battle", null);
        response.EnsureSuccessStatusCode();

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement battle = body.GetProperty("battle");
        JsonElement events = battle.GetProperty("events");

        Assert.Equal(created.RunId, body.GetProperty("run").GetProperty("runId").GetString());
        Assert.Equal(JsonValueKind.String, battle.GetProperty("outcome").ValueKind);
        Assert.NotEmpty(battle.GetProperty("player").EnumerateArray());
        Assert.NotEmpty(battle.GetProperty("opponent").EnumerateArray());
        Assert.Equal("start", events[0].GetProperty("type").GetString());

        Assert.All(events.EnumerateArray(), battleEvent =>
            Assert.Equal(JsonValueKind.String, battleEvent.GetProperty("cause").GetProperty("kind").ValueKind));
    }

    [Fact]
    public async Task FightBattle_StoresTheRunsTeamAsAGhost()
    {
        Run created = await CreateRunAsync();

        await Client.PostAsync($"/runs/{created.RunId}/battle", null);

        IReadOnlyList<Ghost> stage = await Ghosts.FindOpponentsAsync(1, "nobody", CancellationToken.None);

        Ghost mine = Assert.Single(stage, ghost => ghost.RunId == created.RunId);
        Assert.Equal(created.Units, mine.Units);
    }

    [Fact]
    public async Task FightBattle_WhenEveryStageIsFought_RejectsTheNextBattle()
    {
        Run created = await CreateRunAsync();

        for (int stage = 0; stage < Economy.TotalStages; stage++)
        {
            HttpResponseMessage fought = await Client.PostAsync($"/runs/{created.RunId}/battle", null);
            fought.EnsureSuccessStatusCode();
        }

        HttpResponseMessage response = await Client.PostAsync($"/runs/{created.RunId}/battle", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FightBattle_WhenTheRunIsUnknown_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.PostAsync("/runs/missing/battle", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PostRunAsync()
    {
        HttpResponseMessage response = await Client.PostAsync("/runs", null);
        response.EnsureSuccessStatusCode();

        return response;
    }

    private async Task<Run> CreateRunAsync()
    {
        HttpResponseMessage response = await PostRunAsync();

        return (await response.Content.ReadFromJsonAsync<Run>(Json))!;
    }
}