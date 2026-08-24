using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;
using Api.Combat.Effects;

namespace Api.Combat.Tests;

public class ValueTests
{
    private static readonly StartEvent Start = new();

    [Fact]
    public void Literal_ResolvesToItsAmount()
    {
        Board board = Boards.ThreeVersusThree();

        int resolved = Literal.Of(3)
            .Resolve(new Context(board, Boards.Find(board, 0)), Start, Boards.Find(board, 1));

        Assert.Equal(3, resolved);
    }

    [Fact]
    public void UnitStat_OverSelf_ReadsTheOwner()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 0).Attack = 5;

        int resolved = new UnitStat<StartEvent> { Subject = One<StartEvent>.Of(new Self()), Stat = Stat.Attack }
            .Resolve(new Context(board, Boards.Find(board, 0)), Start, Boards.Find(board, 1));

        Assert.Equal(5, resolved);
    }

    [Fact]
    public void RecipientStat_ReadsTheUnitBeingApplied()
    {
        Board board = Boards.ThreeVersusThree();
        Unit recipient = Boards.Find(board, 1);
        recipient.Health = 4;

        int resolved = new RecipientStat { Stat = Stat.Health }
            .Resolve(new Context(board, Boards.Find(board, 0)), Start, recipient);

        Assert.Equal(4, resolved);
    }

    [Fact]
    public void UnitStat_OverEventSource_ReadsTheUnitThatCausedIt()
    {
        Board board = Boards.ThreeVersusThree();
        Boards.Find(board, 3).Attack = 7;

        UnitStat<UnitHurtEvent> value = new()
        {
            Subject = One<UnitHurtEvent>.Of(new EventUnit<UnitHurtEvent> { Participant = new EventSource() }),
            Stat = Stat.Attack
        };

        int resolved = value.Resolve(
            new Context(board, Boards.Find(board, 0)),
            Boards.HurtEvent(board, 3, 5),
            Boards.Find(board, 1));

        Assert.Equal(7, resolved);
    }

    [Fact]
    public void Damage_ReadingTheOwnersAttack_DealsThatMuch()
    {
        Board board = Boards.ThreeVersusThree();
        Unit owner = Boards.Find(board, 0);
        Unit recipient = Boards.Find(board, 1);
        owner.Attack = 3;
        recipient.Health = 10;

        Damage<StartEvent> damage = new() { Value = new UnitStat<StartEvent> { Subject = One<StartEvent>.Of(new Self()), Stat = Stat.Attack } };
        IReadOnlyList<BattleEvent> events = damage.Apply(new Context(board, owner), Start, recipient);

        Assert.Equal(7, recipient.Health);
        Assert.Equal([new UnitHurtEvent { Source = owner, Target = recipient, Value = 3 }], events);
    }
}
