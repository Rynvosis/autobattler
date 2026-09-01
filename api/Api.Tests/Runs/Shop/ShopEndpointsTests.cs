using System.Net;
using System.Net.Http.Json;
using Api.Runs;
using Api.Teams;

namespace Api.Tests.Runs.Shop;

public sealed class ShopEndpointsTests(ApiFixture fixture) : RunApiTests(fixture)
{
    [Fact]
    public async Task Buy_NullsTheSlotItTookAndChargesGold()
    {
        Run created = await CreateRunAsync();

        Run bought = await BuyAsync(created, 0);

        Assert.Null(bought.Shop[0]);
        Assert.NotNull(bought.Shop[1]);
        Assert.Equal(created.Gold - Economy.UnitCost, bought.Gold);
        Assert.Equal(created.Shop[0], Assert.Single(bought.Units));
    }

    [Fact]
    public async Task Reroll_ReplacesEveryOfferAndChargesGold()
    {
        Run created = await CreateRunAsync();
        Run bought = await BuyAsync(created, 0);

        Run rerolled = await ReadRunAsync(PostAsync(bought, "shop/roll", new { version = bought.Version }));

        Assert.Equal(bought.Gold - Economy.RerollCost, rerolled.Gold);
        Assert.Equal(Economy.ShopSize, rerolled.Shop.Count);
        Assert.All(rerolled.Shop, Assert.NotNull);
    }

    [Fact]
    public async Task Duplicate_CopiesTheUnitAtItsBaseStats()
    {
        Run run = await CreateRunAsync();
        TeamUnit offer = run.Shop[0]!;

        run = await BuyAsync(run, 0);
        run = await SetGoldAsync(run, Economy.UpgradeCost + Economy.DuplicateCost);
        run = await ReadRunAsync(PostAsync(run, "shop/upgrade", new { version = run.Version, teamSlot = 0 }));
        run = await ReadRunAsync(PostAsync(run, "shop/duplicate", new { version = run.Version, teamSlot = 0 }));

        Assert.Equal(offer.Attack + 1, run.Units[0].Attack);
        Assert.Equal(offer, run.Units[1]);
    }

    [Fact]
    public async Task Duplicate_TwiceInOneStage_IsRefused()
    {
        Run run = await CreateRunAsync();
        run = await BuyAsync(run, 0);
        run = await SetGoldAsync(run, Economy.DuplicateCost * 2);
        run = await ReadRunAsync(PostAsync(run, "shop/duplicate", new { version = run.Version, teamSlot = 0 }));

        await AssertRefusedAsync(
            PostAsync(run, "shop/duplicate", new { version = run.Version, teamSlot = 0 }),
            "alreadyDuplicated");
    }

    [Fact]
    public async Task Sell_ThenUpgrade_SpendsTheCreditRatherThanGold()
    {
        Run run = await CreateRunAsync();
        run = await BuyAsync(run, 0);
        run = await BuyAsync(run, 1);

        run = await ReadRunAsync(PostAsync(run, "team/sell", new { version = run.Version, teamSlot = 1 }));

        Assert.Equal(1, run.UpgradeCredits);

        int goldBeforeUpgrade = run.Gold;
        TeamUnit before = run.Units[0];

        run = await ReadRunAsync(PostAsync(run, "shop/upgrade", new { version = run.Version, teamSlot = 0 }));

        Assert.Equal(0, run.UpgradeCredits);
        Assert.Equal(goldBeforeUpgrade, run.Gold);
        Assert.Equal(before.Attack + 1, run.Units[0].Attack);
        Assert.Equal(before.Health + 1, run.Units[0].Health);
    }

    [Fact]
    public async Task Sell_WhenOneUnitRemains_IsRefused()
    {
        Run run = await CreateRunAsync();
        run = await BuyAsync(run, 0);

        await AssertRefusedAsync(
            PostAsync(run, "team/sell", new { version = run.Version, teamSlot = 0 }),
            "lastUnit");
    }

    [Fact]
    public async Task Upgrade_WithNoCreditsAndTooLittleGold_IsRefused()
    {
        Run run = await CreateRunAsync();

        for (int slot = 0; slot < Economy.ShopSize; slot++) run = await BuyAsync(run, slot);

        await AssertRefusedAsync(
            PostAsync(run, "shop/upgrade", new { version = run.Version, teamSlot = 0 }),
            "insufficientGold");
    }

    [Fact]
    public async Task Reorder_TakesEachUnitFromTheSlotNamed()
    {
        Run run = await CreateRunAsync();

        for (int slot = 0; slot < Economy.ShopSize; slot++) run = await BuyAsync(run, slot);

        IReadOnlyList<TeamUnit> before = run.Units;

        run = await ReadRunAsync(
            PostAsync(run, "team/reorder", new { version = run.Version, order = new[] { 2, 0, 1 } }));

        Assert.Equal([before[2], before[0], before[1]], run.Units);
    }

    [Fact]
    public async Task Reorder_WhenTheOrderRepeatsASlot_IsRefused()
    {
        Run run = await CreateRunAsync();
        run = await BuyAsync(run, 0);
        run = await BuyAsync(run, 1);

        await AssertRefusedAsync(
            PostAsync(run, "team/reorder", new { version = run.Version, order = new[] { 0, 0 } }),
            "notAPermutation");
    }

    [Fact]
    public async Task Buy_WhenTheVersionIsStale_ReturnsTheStoredRun()
    {
        Run created = await CreateRunAsync();

        await BuyAsync(created, 0);

        HttpResponseMessage response =
            await PostAsync(created, "shop/buy", new { version = created.Version, shopSlot = 1 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        Run stored = (await response.Content.ReadFromJsonAsync<Run>(Json))!;

        Assert.Equal(created.Version + 1, stored.Version);
    }

    [Fact]
    public async Task Buy_WhenTheRunIsUnknown_ReturnsNotFound()
    {
        HttpResponseMessage response =
            await Client.PostAsJsonAsync("/runs/missing/shop/buy", new { version = 1, shopSlot = 0 }, Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}