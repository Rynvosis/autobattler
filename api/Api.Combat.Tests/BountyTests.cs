using Api.Content;

namespace Api.Combat.Tests;

// A bounty is the one effect that leaves the battle: combat only records that it was earned,
// and the run decides what it is worth.
public class BountyTests
{
    private static Unit Monster(int id, UnitDefinition definition) =>
        new() { Id = id, Kind = definition.Kind, Attack = definition.Attack, Health = definition.Health };

    [Fact]
    public void Coinbug_WhenItDies_PaysItsBounty()
    {
        Unit coinbug = Monster(0, Monsters.Coinbug);
        Unit killer = Boards.Unit(1, attack: 9, health: 99);

        BattleResult result = Battle.Resolve(new Team([coinbug]), new Team([killer]), Monsters.Roster);

        UnitBountyEvent bounty = Assert.Single(result.Events.OfType<UnitBountyEvent>());
        Assert.Equal(2, bounty.Value);
        Assert.Same(coinbug, bounty.Target);
    }

    [Fact]
    public void Vulture_WhenItKills_PaysItsBounty()
    {
        Unit vulture = Monster(0, Monsters.Vulture);
        Unit prey = Boards.Unit(1, attack: 0, health: 1);

        BattleResult result = Battle.Resolve(new Team([vulture]), new Team([prey]), Monsters.Roster);

        UnitBountyEvent bounty = Assert.Single(result.Events.OfType<UnitBountyEvent>());
        Assert.Equal(1, bounty.Value);
        Assert.Same(vulture, bounty.Target);
    }

    // The corpse-reach exemption is the bounty's alone.
    [Fact]
    public void StatChange_OnADeadUnit_DoesNothing()
    {
        Unit ghoul = Monster(0, Monsters.Ghoul);
        Unit killer = Boards.Unit(1, attack: 99, health: 99);

        Battle.Resolve(new Team([ghoul]), new Team([killer]), Monsters.Roster);

        Assert.True(ghoul.Dead);
        Assert.Equal(Monsters.Ghoul.Attack, ghoul.Attack);
    }
}
