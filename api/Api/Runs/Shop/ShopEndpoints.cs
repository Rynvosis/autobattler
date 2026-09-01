namespace Api.Runs.Shop;

public static class ShopEndpoints
{
    public static void MapMoves(this WebApplication app)
    {
        MapMove<MutationRequest>("/runs/{runId}/shop/roll", (run, _) => Moves.Reroll(run));
        MapMove<ShopSlotRequest>("/runs/{runId}/shop/buy", (run, body) => Moves.Buy(run, body.ShopSlot));
        MapMove<TeamSlotRequest>("/runs/{runId}/shop/duplicate", (run, body) => Moves.Duplicate(run, body.TeamSlot));
        MapMove<TeamSlotRequest>("/runs/{runId}/shop/upgrade", (run, body) => Moves.Upgrade(run, body.TeamSlot));
        MapMove<TeamSlotRequest>("/runs/{runId}/team/sell", (run, body) => Moves.Sell(run, body.TeamSlot));
        MapMove<ReorderRequest>("/runs/{runId}/team/reorder", (run, body) => Moves.Reorder(run, body.Order));

        return;

        void MapMove<TBody>(string route, Func<Run, TBody, MoveOutcome> move) where TBody : MutationRequest
        {
            app.MapPost(route, (string runId, TBody body, RunStore runs, CancellationToken cancellationToken) =>
                ApplyAndStoreAsync(runId, body.Version, run => move(run, body), runs, cancellationToken));
        }
    }

    private static async Task<IResult> ApplyAndStoreAsync(
        string runId,
        int version,
        Func<Run, MoveOutcome> move,
        RunStore runs,
        CancellationToken cancellationToken)
    {
        Run? run = await runs.GetAsync(runId, cancellationToken);

        if (run is null) return Results.NotFound();

        if (run.Version != version) return Results.Json(run, statusCode: StatusCodes.Status409Conflict);

        MoveOutcome outcome = move(run);

        if (outcome.Run is null) return Results.BadRequest(new { error = outcome.Error });

        return Results.Ok(await runs.UpdateAsync(outcome.Run, cancellationToken));
    }
}