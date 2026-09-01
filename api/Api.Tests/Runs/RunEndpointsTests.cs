using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Ghosts;
using Api.Runs;

namespace Api.Tests.Runs;

public sealed class RunEndpointsTests(ApiFixture fixture) : RunApiTests(fixture)
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
        Assert.Empty(run.Units);
        Assert.Equal(Economy.ShopSize, run.Shop.Count);
        Assert.False(run.Finished);
    }

    [Fact]
    public async Task CreateRun_WritesAUnitKindAsAString()
    {
        HttpResponseMessage response = await PostNewRunAsync();

        JsonElement run = await response.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement kind = run.GetProperty("shop")[0].GetProperty("kind");

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
        Run created = await CreateArmedRunAsync();

        HttpResponseMessage response = await BattleAsync(created);
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
        Run created = await CreateArmedRunAsync();

        HttpResponseMessage response = await BattleAsync(created);
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
        Run created = await CreateArmedRunAsync();

        await BattleAsync(created);

        IReadOnlyList<Ghost> stage = await Ghosts.FindOpponentsAsync(1, "nobody", CancellationToken.None);

        Ghost mine = Assert.Single(stage, ghost => ghost.RunId == created.RunId);
        Assert.Equal(created.Units, mine.Units);
    }

    [Fact]
    public async Task FightBattle_WhenEveryStageIsFought_RejectsTheNextBattle()
    {
        Run run = await CreateArmedRunAsync();

        for (int stage = 0; stage < Economy.TotalStages; stage++)
        {
            HttpResponseMessage fought = await BattleAsync(run);
            fought.EnsureSuccessStatusCode();

            JsonElement body = await fought.Content.ReadFromJsonAsync<JsonElement>();
            run = body.GetProperty("run").Deserialize<Run>(Json)!;
        }

        HttpResponseMessage response = await BattleAsync(run);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FightBattle_WhenTheRunIsUnknown_ReturnsNotFound()
    {
        HttpResponseMessage response =
            await Client.PostAsJsonAsync("/runs/missing/battle", new { version = 1 }, Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<HttpResponseMessage> BattleAsync(Run run)
    {
        return PostAsync(run, "battle", new { version = run.Version });
    }

    private async Task<Run> CreateArmedRunAsync()
    {
        return await BuyAsync(await CreateRunAsync(), 0);
    }
}