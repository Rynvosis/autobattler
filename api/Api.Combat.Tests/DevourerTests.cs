using Api.Combat.Abilities;
using Api.Combat.Scopes;
using Api.Combat.Effects;

namespace Api.Combat.Tests;

public class DevourerTests
{
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
    public void Devourer_GainsTheStatsOfTheAllyAheadThenKillsIt()
    {
        Unit food = new() { Id = 0, Attack = 3, Health = 4, Ability = null };
        Unit devourer = new() { Id = 1, Attack = 3, Health = 5, Ability = Devourer() };
        Unit dummy = new() { Id = 2, Attack = 0, Health = 99, Ability = null };

        BattleResult result = Battle.Resolve(new Team([food, devourer]), new Team([dummy]));

        Assert.Equal(6, devourer.Attack);
        Assert.True(food.Dead);
        Assert.Contains(result.Events.OfType<UnitKillEvent>(), kill => ReferenceEquals(kill.Target, food));
    }
}
