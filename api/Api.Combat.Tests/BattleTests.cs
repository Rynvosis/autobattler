namespace Api.Combat.Tests;

public class BattleTests
{
    [Theory]
    [InlineData(1, 1, 1, 1, BattleOutcome.Draw)]
    [InlineData(0, 1, 0, 1, BattleOutcome.Draw)]
    [InlineData(1, 3, 1, 3, BattleOutcome.Draw)]
    [InlineData(1, 2, 1, 1, BattleOutcome.Win)]
    [InlineData(3, 1, 0, 2, BattleOutcome.Win)]
    [InlineData(1, 1, 1, 2, BattleOutcome.Loss)]
    public void HeadToHead(int playerAttack, int playerHealth, int ghostAttack, int ghostHealth,
        BattleOutcome expected)
    {
        Unit player = new Unit { Id = 0, Attack = playerAttack, MaxHealth = playerHealth };
        Unit ghost = new Unit { Id = 1, Attack = ghostAttack, MaxHealth = ghostHealth };

        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]));

        Assert.Equal(expected, result.Outcome);
    }
}
