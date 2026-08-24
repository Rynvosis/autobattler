using Api.Combat.Battlefield;
using Api.Combat.Battles;
using Api.Content;
using Api.Ghosts;
using Api.Teams;

namespace Api.Runs;

public static class RunEndpoints
{
    private const int RunLifetimeHours = 48;

    public static void MapRuns(this WebApplication app)
    {
        app.MapPost("/runs", CreateRun);
        app.MapPost("/runs/{runId}/battle", FightBattle);
    }

    private static async Task<IResult> CreateRun(RunStore runs, CancellationToken cancellationToken)
    {
        Run run = StartRun(Guid.NewGuid().ToString());

        await runs.PutAsync(run, cancellationToken);

        return Results.Created($"/runs/{run.RunId}", run);
    }

    private static async Task<IResult> FightBattle(
        string runId,
        RunStore runs,
        GhostStore ghosts,
        CancellationToken cancellationToken)
    {
        Run? run = await runs.GetAsync(runId, cancellationToken);

        if (run is null)
        {
            return Results.NotFound();
        }

        Team player = TeamUnits.ToTeam(run.Units, 0);
        Team ghost = await FindGhostTeamAsync(ghosts, run, cancellationToken);

        BattleResult result = Battle.Resolve(player, ghost, Monsters.Roster);

        // TODO: apply the outcome to the run, store a ghost of the team, and write both back.
        return Results.Ok(result);
    }

    private static Run StartRun(string runId)
    {
        return new Run
        {
            RunId = runId,
            Version = 1,
            Victories = 0,
            Gold = Economy.StartingGold,
            Stage = 1,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(RunLifetimeHours),
            Units = StartingTeam()
        };
    }

    // TODO: the shop fills a team. Until it exists, a run starts with a fixed one.
    private static IReadOnlyList<TeamUnit> StartingTeam()
    {
        return
        [
            UnitOf(Monsters.Golem),
            UnitOf(Monsters.Goblin)
        ];
    }

    private static TeamUnit UnitOf(UnitDefinition definition)
    {
        return new TeamUnit
        {
            Kind = definition.Kind,
            Attack = definition.Attack,
            Health = definition.Health
        };
    }

    // TODO: the opponent when the stage holds no ghost.
    private static Task<Team> FindGhostTeamAsync(GhostStore ghosts, Run run, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}