using Api.Content;

namespace Api.Combat.Tests;

// Abilities come from Api.Content, so the ability under test is the one that ships.
public class MonsterTests
{
    private static Unit Monster(int id, UnitDefinition definition)
    {
        return new Unit
        {
            Id = id,
            Kind = definition.Kind,
            Attack = definition.Attack,
            Health = definition.Health
        };
    }

    private static Unit Dummy(int id, int attack, int health)
    {
        return new Unit { Id = id, Kind = new Kind("dummy"), Attack = attack, Health = health };
    }

    [Fact]
    public void Deathcap_WhenItDies_KillsTheEnemyHead()
    {
        Unit deathcap = Monster(0, Monsters.Deathcap);
        Unit golem = Monster(1, Monsters.Golem);

        BattleResult result = Battle.Resolve(new Team([deathcap]), new Team([golem]), Monsters.Roster);

        Assert.Equal(BattleOutcome.Draw, result.Outcome);
        Assert.True(golem.Dead);
        Assert.Contains(result.Events.OfType<UnitKillEvent>(), kill => ReferenceEquals(kill.Source, deathcap));
    }

    [Fact]
    public void Deathcap_WhenTheEnemyHeadDiesInTheSameSubtick_KillsTheNextEnemy()
    {
        Unit deathcap = Monster(0, Monsters.Deathcap);
        Unit head = Dummy(1, 4, 1);
        Unit second = Dummy(2, 5, 10);

        BattleResult result = Battle.Resolve(new Team([deathcap]), new Team([head, second]), Monsters.Roster);

        Assert.True(head.Dead);
        Assert.True(second.Dead);
        Assert.Contains(result.Events.OfType<UnitKillEvent>(), kill => ReferenceEquals(kill.Target, second));
    }

    [Fact]
    public void Devourer_OnStart_GainsTheStatsOfTheAllyAheadThenKillsIt()
    {
        Unit golem = Monster(0, Monsters.Golem);
        Unit devourer = Monster(1, Monsters.Devourer);
        Unit enemy = Dummy(2, 0, 99);

        BattleResult result = Battle.Resolve(new Team([golem, devourer]), new Team([enemy]), Monsters.Roster);

        Assert.Equal(8, devourer.Attack);
        Assert.True(golem.Dead);
        Assert.Contains(result.Events.OfType<UnitKillEvent>(), kill => ReferenceEquals(kill.Target, golem));
    }

    [Fact]
    public void Ghoul_WhenAnEnemyDies_GainsTwoOfEachStat()
    {
        Unit ghoul = Monster(0, Monsters.Ghoul);
        Unit enemy = Dummy(1, 0, 1);

        BattleResult result = Battle.Resolve(new Team([ghoul]), new Team([enemy]), Monsters.Roster);

        Assert.Equal(BattleOutcome.Win, result.Outcome);
        Assert.Equal(4, ghoul.Attack);
        Assert.Equal(7, ghoul.Health);
    }

    [Fact]
    public void Wyrm_WhenItAttacks_DealsItsAttackToTheSecondEnemy()
    {
        Unit wyrm = Monster(0, Monsters.Wyrm);
        Unit head = Dummy(1, 0, 2);
        Unit second = Dummy(2, 0, 2);

        BattleResult result = Battle.Resolve(new Team([wyrm]), new Team([head, second]), Monsters.Roster);

        Assert.True(second.Dead);
        Assert.Contains(
            result.Events.OfType<UnitHurtEvent>(),
            hurt => ReferenceEquals(hurt.Source, wyrm) && ReferenceEquals(hurt.Target, second) && hurt.Value == 2);
    }

    [Fact]
    public void Wyrm_WhenItAttacks_CreditsItsStrikeAndItsAbilitySeparately()
    {
        Unit wyrm = Monster(0, Monsters.Wyrm);
        Unit head = Dummy(1, 0, 2);
        Unit second = Dummy(2, 0, 2);

        BattleResult result = Battle.Resolve(new Team([wyrm]), new Team([head, second]), Monsters.Roster);

        UnitHurtEvent strike = Assert.Single(
            result.Events.OfType<UnitHurtEvent>(),
            hurt => ReferenceEquals(hurt.Target, head));

        UnitHurtEvent ability = Assert.Single(
            result.Events.OfType<UnitHurtEvent>(),
            hurt => ReferenceEquals(hurt.Target, second));

        Assert.Equal(Cause.Attack(wyrm), strike.Cause);
        Assert.Equal(Cause.Ability(wyrm), ability.Cause);
    }

    [Fact]
    public void Vampire_WhenItAttacks_GainsOneHealth()
    {
        Unit vampire = Monster(0, Monsters.Vampire);
        Unit enemy = Dummy(1, 0, 1);

        Battle.Resolve(new Team([vampire]), new Team([enemy]), Monsters.Roster);

        Assert.Equal(8, vampire.Health);
        Assert.Equal(3, vampire.Attack);
    }

    [Fact]
    public void Goblin_OnStart_GivesEveryGoblinIncludingItselfTwoOfEachStat()
    {
        Unit first = Monster(0, Monsters.Goblin);
        Unit second = Monster(1, Monsters.Goblin);
        Unit golem = Monster(2, Monsters.Golem);
        Unit enemy = Dummy(3, 0, 1);

        Battle.Resolve(new Team([first, second, golem]), new Team([enemy]), Monsters.Roster);

        Assert.Equal(5, first.Attack);
        Assert.Equal(8, first.Health);
        Assert.Equal(5, second.Attack);
        Assert.Equal(8, second.Health);
        Assert.Equal(5, golem.Attack);
    }

    [Fact]
    public void Wraithblade_WhenItDies_GivesItsAttackToTheAllyBehind()
    {
        Unit wraithblade = Monster(0, Monsters.Wraithblade);
        Unit golem = Monster(1, Monsters.Golem);
        Unit killer = Dummy(2, 3, 95);

        Battle.Resolve(new Team([wraithblade, golem]), new Team([killer]), Monsters.Roster);

        Assert.True(wraithblade.Dead);
        Assert.Equal(10, golem.Attack);
    }
}
