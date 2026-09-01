using Api.Combat.Battles;

namespace Api.Runs;

public static class Economy
{
    public const int TotalStages = 5;
    public const int TeamSize = 5;
    public const int ShopSize = 3;

    public const int StartingGold = 7;

    public const int WinGold = 6;
    public const int DrawGold = 5;
    public const int LossGold = 4;

    public const int UnitCost = 2;
    public const int RerollCost = 1;
    public const int UpgradeCost = 2;
    public const int DuplicateCost = 6;

    public static int GoldFor(BattleOutcome outcome)
    {
        return outcome switch
        {
            BattleOutcome.Win => WinGold,
            BattleOutcome.Draw => DrawGold,
            BattleOutcome.Loss => LossGold,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };
    }
}
