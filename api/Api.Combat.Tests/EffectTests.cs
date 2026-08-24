using Api.Combat.Effects;

namespace Api.Combat.Tests;

public class EffectTests
{
    private static readonly StartEvent Start = new();

    private static (Context Context, Unit Source, Unit Recipient) OnBoard()
    {
        Board board = Boards.ThreeVersusThree();
        Unit source = Boards.Find(board, 0);
        Unit recipient = Boards.Find(board, 1);

        return (new Context(board, source), source, recipient);
    }

    private static Damage<StartEvent> Damaging(int amount)
    {
        return new Damage<StartEvent> { Value = Literal.Of(amount) };
    }

    private static StatChange<StartEvent> Changing(int attack, int health)
    {
        return new StatChange<StartEvent> { Attack = Literal.Of(attack), Health = Literal.Of(health) };
    }

    [Fact]
    public void Damage_LiveTarget_LowersHealthAndEmitsHurt()
    {
        (Context context, Unit source, Unit recipient) = OnBoard();

        IReadOnlyList<BattleEvent> events = Damaging(1).Apply(context, Start, recipient);

        Assert.Equal(0, recipient.Health);
        Assert.Equal([new UnitHurtEvent { Source = source, Target = recipient, Value = 1 }], events);
    }

    [Fact]
    public void Damage_ZeroValue_EmitsNothing()
    {
        (Context context, Unit _, Unit recipient) = OnBoard();

        IReadOnlyList<BattleEvent> events = Damaging(0).Apply(context, Start, recipient);

        Assert.Equal(1, recipient.Health);
        Assert.Empty(events);
    }

    [Fact]
    public void StatChange_LiveTarget_RaisesBothStatsAndEmitsBothChanges()
    {
        (Context context, Unit source, Unit recipient) = OnBoard();

        IReadOnlyList<BattleEvent> events = Changing(1, 2).Apply(context, Start, recipient);

        Assert.Equal(2, recipient.Attack);
        Assert.Equal(3, recipient.Health);
        Assert.Equal(
            [
                new UnitAttackChangeEvent { Source = source, Target = recipient, Value = 1 },
                new UnitHealthChangeEvent { Source = source, Target = recipient, Value = 2 }
            ],
            events);
    }

    [Fact]
    public void StatChange_HealthOnly_EmitsNoAttackChange()
    {
        (Context context, Unit source, Unit recipient) = OnBoard();

        IReadOnlyList<BattleEvent> events = Changing(0, 1).Apply(context, Start, recipient);

        Assert.Equal([new UnitHealthChangeEvent { Source = source, Target = recipient, Value = 1 }], events);
    }

    [Fact]
    public void StatChange_NegativeHealth_LeavesTargetForTheDeathPass()
    {
        (Context context, Unit _, Unit recipient) = OnBoard();

        Changing(0, -1).Apply(context, Start, recipient);

        Assert.Equal(0, recipient.Health);
        Assert.False(recipient.Dead);
    }

    [Fact]
    public void StatChange_NoDeltas_EmitsNothing()
    {
        (Context context, Unit _, Unit recipient) = OnBoard();

        Assert.Empty(Changing(0, 0).Apply(context, Start, recipient));
    }
}
