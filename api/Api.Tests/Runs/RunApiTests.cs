using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Runs;

namespace Api.Tests.Runs;

public abstract class RunApiTests(ApiFixture fixture) : ApiTests(fixture)
{
    protected Task<HttpResponseMessage> PostAsync(Run run, string path, object body)
    {
        return Client.PostAsJsonAsync($"/runs/{run.RunId}/{path}", body, Json);
    }

    protected async Task<HttpResponseMessage> PostNewRunAsync()
    {
        HttpResponseMessage response = await Client.PostAsync("/runs", null);
        response.EnsureSuccessStatusCode();

        return response;
    }

    protected async Task<Run> CreateRunAsync()
    {
        return await ReadRunAsync(PostNewRunAsync());
    }

    // A run starts with no units, so tests build a team by buying.
    protected async Task<Run> BuyAsync(Run run, int shopSlot)
    {
        return await ReadRunAsync(PostAsync(run, "shop/buy", new { version = run.Version, shopSlot }));
    }

    // Starting gold buys three units, so richer states are seeded through the store rather than played out.
    protected Task<Run> SetGoldAsync(Run run, int gold)
    {
        return Service<RunStore>().UpdateAsync(run with { Gold = gold }, CancellationToken.None);
    }

    protected async Task<Run> ReadRunAsync(Task<HttpResponseMessage> posting)
    {
        HttpResponseMessage response = await posting;
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<Run>(Json))!;
    }

    protected static async Task AssertRefusedAsync(Task<HttpResponseMessage> posting, string error)
    {
        HttpResponseMessage response = await posting;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(error, body.GetProperty("error").GetString());
    }
}