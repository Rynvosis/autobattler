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

    [Fact]
    public void TwoVersusOne()
    {
        Unit player1 = new Unit { Id = 0, Attack = 1, MaxHealth = 2 };
        Unit player2 = new Unit { Id = 1, Attack = 1, MaxHealth = 2 };
        Unit ghost = new Unit { Id = 2, Attack = 2, MaxHealth = 3 };

        BattleResult result = Battle.Resolve(new Team([player1, player2]), new Team([ghost]));

        Assert.Equal(BattleOutcome.Loss, result.Outcome);
    }


    [Fact]
    public void MutualWipe_EmitsEventsInScheduleOrder()
    {
        
        Unit player = new() { Id = 0, Attack = 1, MaxHealth = 2 };
        Unit ghost = new() { Id = 1, Attack = 1, MaxHealth = 2 };
        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]));
        
        BattleEvent[] expected =
        [
            new() { Kind = EventKind.OnStart, Tick = 0, Subtick = 0 },

            new() { Kind = EventKind.OnUnitAttack, Tick = 1, Subtick = 0, Source = player.Id, Target = ghost.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitAttack, Tick = 1, Subtick = 0, Source = ghost.Id, Target = player.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitHurt,   Tick = 1, Subtick = 1, Source = player.Id, Target = ghost.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitHurt,   Tick = 1, Subtick = 1, Source = ghost.Id, Target = player.Id, Value = 1 },

            new() { Kind = EventKind.OnUnitAttack, Tick = 2, Subtick = 0, Source = player.Id, Target = ghost.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitAttack, Tick = 2, Subtick = 0, Source = ghost.Id, Target = player.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitHurt,   Tick = 2, Subtick = 1, Source = player.Id, Target = ghost.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitHurt,   Tick = 2, Subtick = 1, Source = ghost.Id, Target = player.Id, Value = 1 },
            new() { Kind = EventKind.OnUnitFaint,  Tick = 2, Subtick = 1, Target = player.Id },
            new() { Kind = EventKind.OnUnitFaint,  Tick = 2, Subtick = 1, Target = ghost.Id },
        ];
        
        Assert.Equal(expected, result.Events);
    }
}
