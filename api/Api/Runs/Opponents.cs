using Api.Combat.Battlefield;
using Api.Ghosts;
using Api.Teams;

namespace Api.Runs;

public static class Opponents
{
    private const int MinimumCandidates = 5;

    public static async Task<Team> FindOrCreateTeamAsync(
        GhostStore ghosts,
        Run run,
        CancellationToken cancellationToken)
    {
        List<Ghost> candidates = [.. await ghosts.FindOpponentsAsync(run.Stage, run.RunId, cancellationToken)];
        HashSet<string> stored = [.. candidates.Select(candidate => candidate.RunId)];

        List<Ghost> fillers = [];

        for (int index = 0; candidates.Count + fillers.Count < MinimumCandidates; index++)
        {
            string fillerId = FillerGhosts.IdFor(run.Stage, index);

            if (!stored.Add(fillerId)) continue;

            fillers.Add(new Ghost
            {
                Stage = run.Stage,
                RunId = fillerId,
                ExpiresAt = run.ExpiresAt,
                Units = FillerUnits.For(run.Stage)
            });
        }

        if (fillers.Count > 0)
        {
            await Task.WhenAll(fillers.Select(filler => ghosts.PutAsync(filler, cancellationToken)));
            candidates.AddRange(fillers);
        }

        Ghost opponent = candidates[Random.Shared.Next(candidates.Count)];

        return TeamUnits.ToTeam(opponent.Units, run.Units.Count);
    }
}