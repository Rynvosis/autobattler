using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;
using Api.Combat.Effects;

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
    public void OneVersusOne_ReturnsExpectedOutcome(int playerAttack, int playerHealth, int ghostAttack,
        int ghostHealth,
        BattleOutcome expected)
    {
        Unit player = new() { Id = 0, Attack = playerAttack, Health = playerHealth, Ability = null };
        Unit ghost = new() { Id = 1, Attack = ghostAttack, Health = ghostHealth, Ability = null };

        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]));

        Assert.Equal(expected, result.Outcome);
    }

    [Fact]
    public void TwoVersusOne_GhostSurvives_ReturnsLoss()
    {
        Unit player1 = new() { Id = 0, Attack = 1, Health = 2, Ability = null };
        Unit player2 = new() { Id = 1, Attack = 1, Health = 2, Ability = null };
        Unit ghost = new() { Id = 2, Attack = 2, Health = 3, Ability = null };

        BattleResult result = Battle.Resolve(new Team([player1, player2]), new Team([ghost]));

        Assert.Equal(BattleOutcome.Loss, result.Outcome);
    }


    [Fact]
    public void GhostRetaliatesOnHurt_ReturnsDraw()
    {
        Ability retaliate = new()
        {
            Trigger = new TargetTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            Effect = new Damage { Value = 1 },
            Scopes = [new EventSourceScope()]
        };

        Unit player = new() { Id = 0, Attack = 2, Health = 2, Ability = null };
        Unit ghost = new() { Id = 1, Attack = 1, Health = 2, Ability = retaliate };

        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]));

        Assert.Equal(BattleOutcome.Draw, result.Outcome);
    }

    [Fact]
    public void ChipDamageInAnEarlierTick_EarnsNoKillCredit()
    {
        Ability opener = new()
        {
            Trigger = new RoundTrigger<StartEvent>(),
            Effect = new Damage { Value = 1 },
            Scopes = [new HeadScope { Side = ScopeSide.Enemy, Range = ScopeRange.At(0) }]
        };

        Unit attacker = new() { Id = 0, Attack = 1, Health = 9, Ability = null };
        Unit victim = new() { Id = 1, Attack = 0, Health = 3, Ability = null };
        Unit chipper = new() { Id = 2, Attack = 0, Health = 9, Ability = opener };

        BattleResult result = Battle.Resolve(new Team([attacker, chipper]), new Team([victim]));

        Assert.Contains(result.Events.OfType<UnitHurtEvent>(), hurt => ReferenceEquals(hurt.Source, chipper));

        UnitKillEvent kill = Assert.Single(result.Events.OfType<UnitKillEvent>());
        Assert.Same(attacker, kill.Source);
    }

    [Fact]
    public void MutualWipe_EmitsEventsInScheduleOrder()
    {
        Unit player = new() { Id = 0, Attack = 1, Health = 2, Ability = null };
        Unit ghost = new() { Id = 1, Attack = 1, Health = 2, Ability = null };
        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]));

        BattleEvent[] expected =
        [
            new StartEvent { Tick = 0, Subtick = 0 },

            new UnitAttackEvent { Tick = 1, Subtick = 0, Source = player, Target = ghost, Value = 1 },
            new UnitAttackEvent { Tick = 1, Subtick = 0, Source = ghost, Target = player, Value = 1 },
            new UnitHurtEvent { Tick = 1, Subtick = 1, Source = player, Target = ghost, Value = 1 },
            new UnitHurtEvent { Tick = 1, Subtick = 1, Source = ghost, Target = player, Value = 1 },

            new UnitAttackEvent { Tick = 2, Subtick = 0, Source = player, Target = ghost, Value = 1 },
            new UnitAttackEvent { Tick = 2, Subtick = 0, Source = ghost, Target = player, Value = 1 },
            new UnitHurtEvent { Tick = 2, Subtick = 1, Source = player, Target = ghost, Value = 1 },
            new UnitHurtEvent { Tick = 2, Subtick = 1, Source = ghost, Target = player, Value = 1 },
            new UnitDeathEvent { Tick = 2, Subtick = 1, Target = player },
            new UnitDeathEvent { Tick = 2, Subtick = 1, Target = ghost },
            new UnitKillEvent { Tick = 2, Subtick = 1, Source = ghost, Target = player },
            new UnitKillEvent { Tick = 2, Subtick = 1, Source = player, Target = ghost }
        ];

        Assert.Equal(expected, result.Events);
    }
}
