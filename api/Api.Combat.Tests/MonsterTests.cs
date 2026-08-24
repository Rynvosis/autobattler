using Api.Combat.Abilities;
using Api.Combat.Effects;
using Api.Combat.Scopes;

namespace Api.Combat.Tests;

// Each test builds a monster exactly as DESIGN's roster describes it, at its designed stats,
// and asserts the ability does what the table says.
public class MonsterTests
{
    private static Unit Monster(int id, string kind, int attack, int health)
    {
        return new Unit { Id = id, Kind = new Kind(kind), Attack = attack, Health = health };
    }

    // Deathcap 1/4 — on-death, target self: kill enemy head[0]
    private static Ability Deathcap()
    {
        return new Ability<UnitDeathEvent>
        {
            Trigger = new UnitTrigger<UnitDeathEvent>
            {
                Participant = new EventTarget(),
                Scopes = [Any<BattleEvent>.Of(new Self())]
            },
            Effects =
            [
                new ScopedEffect<UnitDeathEvent>
                {
                    Effect = new Damage<UnitDeathEvent> { Value = new RecipientStat { Stat = Stat.Health } },
                    Scopes =
                    [
                        new Every<UnitDeathEvent>
                        {
                            Relation = new FromHead { Side = ScopeSide.Enemy },
                            Range = ScopeRange.At(0)
                        }
                    ]
                }
            ]
        };
    }

    // Devourer 3/5 — on-start: gain ally ahead[0] stats, kill it
    private static Ability Devourer()
    {
        Ahead<StartEvent> ahead = new() { Anchor = One<StartEvent>.Of(new Self()) };
        One<StartEvent> eaten = One<StartEvent>.Of(ahead);

        return new Ability<StartEvent>
        {
            Trigger = new RoundTrigger<StartEvent>(),
            Effects =
            [
                new ScopedEffect<StartEvent>
                {
                    Effect = new StatChange<StartEvent>
                    {
                        Attack = new UnitStat<StartEvent> { Subject = eaten, Stat = Stat.Attack },
                        Health = new UnitStat<StartEvent> { Subject = eaten, Stat = Stat.Health }
                    },
                    Scopes = [Every<StartEvent>.Of(new Self())]
                },
                new ScopedEffect<StartEvent>
                {
                    Effect = new Damage<StartEvent> { Value = new RecipientStat { Stat = Stat.Health } },
                    Scopes = [new Every<StartEvent> { Relation = ahead, Range = ScopeRange.At(0) }]
                }
            ]
        };
    }

    [Fact]
    public void Deathcap_WhenItDies_KillsTheEnemyHead()
    {
        Unit deathcap = Monster(0, "deathcap", 1, 4);
        Unit golem = Monster(1, "golem", 5, 10);

        BattleResult result = Battle.Resolve(
            new Team([deathcap]),
            new Team([golem]),
            Roster.Of((new Kind("deathcap"), Deathcap())));

        Assert.Equal(BattleOutcome.Draw, result.Outcome);
        Assert.True(golem.Dead);
        Assert.Contains(result.Events.OfType<UnitKillEvent>(), kill => ReferenceEquals(kill.Source, deathcap));
    }

    [Fact]
    public void Devourer_OnStart_GainsTheStatsOfTheAllyAheadThenKillsIt()
    {
        Unit golem = Monster(0, "golem", 5, 10);
        Unit devourer = Monster(1, "devourer", 3, 5);
        Unit dummy = Monster(2, "dummy", 0, 99);

        BattleResult result = Battle.Resolve(
            new Team([golem, devourer]),
            new Team([dummy]),
            Roster.Of((new Kind("devourer"), Devourer())));

        Assert.Equal(8, devourer.Attack);
        Assert.True(golem.Dead);
        Assert.Contains(result.Events.OfType<UnitKillEvent>(), kill => ReferenceEquals(kill.Target, golem));
    }
}
