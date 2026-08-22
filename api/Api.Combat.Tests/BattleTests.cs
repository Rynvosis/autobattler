namespace Api.Combat.Tests;

public class BattleTests
{
    [Fact]
    public void TwoIdenticalUnits_Draw()
    {
        Unit p1 = new Unit { Id = 0, Attack = 1, MaxHealth = 1 };
        Team pTeam = new Team([p1]);
        Unit g1 = new Unit { Id = 1, Attack = 1, MaxHealth = 1 };
        Team gTeam = new Team([g1]);

        BattleResult result = Battle.Resolve(pTeam, gTeam);
        Assert.Equal(BattleOutcome.Draw, result.Outcome);
    }

    [Fact]
    public void OneStrongerUnit_Wins()
    {
        Unit p1 = new Unit { Id = 0, Attack = 1, MaxHealth = 2 };
        Team pTeam = new Team([p1]);
        Unit g1 = new Unit { Id = 1, Attack = 1, MaxHealth = 1 };
        Team gTeam = new Team([g1]);

        BattleResult result = Battle.Resolve(pTeam, gTeam);
        Assert.Equal(BattleOutcome.Win, result.Outcome);
    }
}
