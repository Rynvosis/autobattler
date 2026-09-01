using Api.Battles;
using Api.Combat.Battlefield;
using Api.Combat.Battles;
using Api.Content;
using Api.Ghosts;
using Api.Runs.Shop;
using Api.Teams;

namespace Api.Runs;

public static class RunEndpoints
{
    private const int RunLifetimeHours = 48;

    public static void MapRuns(this WebApplication app)
    {
        app.MapPost("/runs", CreateRun);
        app.MapGet("/runs/{runId}", GetRun);
        app.MapPost("/runs/{runId}/battle", FightBattle);
    }

    private static async Task<IResult> CreateRun(RunStore runs, CancellationToken cancellationToken)
    {
        Run run = await runs.CreateAsync(StartRun(Guid.NewGuid().ToString()), cancellationToken);

        return Results.Created($"/runs/{run.RunId}", run);
    }

    private static async Task<IResult> GetRun(string runId, RunStore runs, CancellationToken cancellationToken)
    {
        Run? run = await runs.GetAsync(runId, cancellationToken);

        if (run is null) return Results.NotFound();

        return Results.Ok(run);
    }

    private static async Task<IResult> FightBattle(
        string runId,
        MutationRequest body,
        RunStore runs,
        GhostStore ghosts,
        CancellationToken cancellationToken)
    {
        Run? run = await runs.GetAsync(runId, cancellationToken);

        if (run is null)
        {
            return Results.NotFound();
        }

        if (run.Version != body.Version)
        {
            return Results.Json(run, statusCode: StatusCodes.Status409Conflict);
        }

        if (run.Finished) return Results.BadRequest(new { error = MoveError.RunFinished });

        if (run.Units.Count == 0) return Results.BadRequest(new { error = MoveError.EmptyTeam });

        Team player = TeamUnits.ToTeam(run.Units, 0);
        Team opponent = await Opponents.FindOrCreateTeamAsync(ghosts, run, cancellationToken);

        // Take both teams before resolving; the battle mutates the units it is given.
        IReadOnlyList<UnitRecord> playerUnits = UnitRecords.From(player);
        IReadOnlyList<UnitRecord> opponentUnits = UnitRecords.From(opponent);

        BattleResult result = Battle.Resolve(player, opponent, Monsters.Roster);

        // The ghost is written first so a crash between the writes leaves a stale ghost rather
        // than an uncredited battle.
        await ghosts.PutAsync(new Ghost
        {
            Stage = run.Stage,
            RunId = run.RunId,
            ExpiresAt = run.ExpiresAt,
            Units = run.Units
        }, cancellationToken);

        Run updated = await runs.UpdateAsync(
            run.AfterBattle(result.Outcome, ShopOffers.Roll()),
            cancellationToken);

        return Results.Ok(new BattleResponse
        {
            Run = updated,
            Battle = new BattleRecord
            {
                Outcome = result.Outcome,
                Player = playerUnits,
                Opponent = opponentUnits,
                Events = EventRecords.From(result.Events)
            }
        });
    }

    private static Run StartRun(string runId)
    {
        return new Run
        {
            RunId = runId,
            Victories = 0,
            Gold = Economy.StartingGold,
            Stage = 1,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(RunLifetimeHours),
            Units = [],
            Shop = ShopOffers.Roll()
        };
    }
}
