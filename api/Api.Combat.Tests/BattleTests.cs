using Api.Combat.Abilities;
using Api.Combat.Effects;
using Api.Combat.Scopes;

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
        Unit player = new() { Id = 0, Kind = new Kind("dummy"), Attack = playerAttack, Health = playerHealth };
        Unit ghost = new() { Id = 1, Kind = new Kind("dummy"), Attack = ghostAttack, Health = ghostHealth };

        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]), Roster.Empty);

        Assert.Equal(expected, result.Outcome);
    }

    [Fact]
    public void TwoVersusOne_GhostSurvives_ReturnsLoss()
    {
        Unit player1 = new() { Id = 0, Kind = new Kind("dummy"), Attack = 1, Health = 2 };
        Unit player2 = new() { Id = 1, Kind = new Kind("dummy"), Attack = 1, Health = 2 };
        Unit ghost = new() { Id = 2, Kind = new Kind("dummy"), Attack = 2, Health = 3 };

        BattleResult result = Battle.Resolve(new Team([player1, player2]), new Team([ghost]), Roster.Empty);

        Assert.Equal(BattleOutcome.Loss, result.Outcome);
    }


    [Fact]
    public void GhostRetaliatesOnHurt_ReturnsDraw()
    {
        Roster roster = Roster.Of((new Kind("retaliate"), Boards.Retaliate()));

        Unit player = new() { Id = 0, Kind = new Kind("dummy"), Attack = 2, Health = 2 };
        Unit ghost = new() { Id = 1, Kind = new Kind("retaliate"), Attack = 1, Health = 2 };

        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]), roster);

        Assert.Equal(BattleOutcome.Draw, result.Outcome);
    }

    [Fact]
    public void ChipDamageInAnEarlierTick_EarnsNoKillCredit()
    {
        Ability opener = new Ability<StartEvent>
        {
            Trigger = new RoundTrigger<StartEvent>(),
            Effects =
            [
                new ScopedEffect<StartEvent>
                {
                    Effect = new Damage<StartEvent> { Value = Literal.Of(1) },
                    Scopes =
                    [
                        new Every<StartEvent>
                        {
                            Relation = new FromHead { Side = ScopeSide.Enemy },
                            Range = ScopeRange.At(0)
                        }
                    ]
                }
            ]
        };

        Unit attacker = new() { Id = 0, Kind = new Kind("dummy"), Attack = 1, Health = 9 };
        Unit victim = new() { Id = 1, Kind = new Kind("dummy"), Attack = 0, Health = 3 };
        Unit chipper = new() { Id = 2, Kind = new Kind("opener"), Attack = 0, Health = 9 };

        BattleResult result = Battle.Resolve(
            new Team([attacker, chipper]),
            new Team([victim]),
            Roster.Of((new Kind("opener"), opener)));

        Assert.Contains(result.Events.OfType<UnitHurtEvent>(), hurt => ReferenceEquals(hurt.Source, chipper));

        UnitKillEvent kill = Assert.Single(result.Events.OfType<UnitKillEvent>());
        Assert.Same(attacker, kill.Source);
    }

    [Fact]
    public void MutualWipe_EmitsEventsInScheduleOrder()
    {
        Unit player = new() { Id = 0, Kind = new Kind("dummy"), Attack = 1, Health = 2 };
        Unit ghost = new() { Id = 1, Kind = new Kind("dummy"), Attack = 1, Health = 2 };
        BattleResult result = Battle.Resolve(new Team([player]), new Team([ghost]), Roster.Empty);

        BattleEvent[] expected =
        [
            new StartEvent { Tick = 0, Subtick = 0, Cause = Cause.Board },

            new UnitAttackEvent
                { Tick = 1, Subtick = 0, Cause = Cause.Attack(player), Source = player, Target = ghost, Value = 1 },
            new UnitAttackEvent
                { Tick = 1, Subtick = 0, Cause = Cause.Attack(ghost), Source = ghost, Target = player, Value = 1 },
            new UnitHurtEvent
                { Tick = 1, Subtick = 1, Cause = Cause.Attack(player), Source = player, Target = ghost, Value = 1 },
            new UnitHurtEvent
                { Tick = 1, Subtick = 1, Cause = Cause.Attack(ghost), Source = ghost, Target = player, Value = 1 },

            new UnitAttackEvent
                { Tick = 2, Subtick = 0, Cause = Cause.Attack(player), Source = player, Target = ghost, Value = 1 },
            new UnitAttackEvent
                { Tick = 2, Subtick = 0, Cause = Cause.Attack(ghost), Source = ghost, Target = player, Value = 1 },
            new UnitHurtEvent
                { Tick = 2, Subtick = 1, Cause = Cause.Attack(player), Source = player, Target = ghost, Value = 1 },
            new UnitHurtEvent
                { Tick = 2, Subtick = 1, Cause = Cause.Attack(ghost), Source = ghost, Target = player, Value = 1 },
            new UnitDeathEvent { Tick = 2, Subtick = 1, Cause = Cause.Board, Target = player },
            new UnitDeathEvent { Tick = 2, Subtick = 1, Cause = Cause.Board, Target = ghost },
            new UnitKillEvent { Tick = 2, Subtick = 1, Cause = Cause.Board, Source = ghost, Target = player },
            new UnitKillEvent { Tick = 2, Subtick = 1, Cause = Cause.Board, Source = player, Target = ghost }
        ];

        Assert.Equal(expected, result.Events);
    }
}
