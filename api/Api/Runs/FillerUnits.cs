using Api.Content;
using Api.Teams;

namespace Api.Runs;

public static class FillerUnits
{
    public static IReadOnlyList<TeamUnit> For(int stage)
    {
        int budget = BudgetFor(stage);
        int count = Math.Min(Economy.TeamSize, budget / Economy.UnitCost);

        List<TeamUnit> units = [.. Random.Shared.GetItems(Monsters.Pool, count).Select(TeamUnits.From)];

        int upgrades = (budget - count * Economy.UnitCost) / Economy.UpgradeCost;

        for (int i = 0; i < upgrades; i++)
        {
            int index = Random.Shared.Next(units.Count);
            units[index] = units[index] with
            {
                Attack = units[index].Attack + 1,
                Health = units[index].Health + 1
            };
        }

        return units;
    }

    private static int BudgetFor(int stage)
    {
        return Economy.StartingGold + Economy.DrawGold * (stage - 1);
    }
}
