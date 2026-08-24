using Api.Combat.Effects;

namespace Api.Combat.Tests;

public class QueuedEffectTests
{
    private static QueuedEffect Damaging(Board board, Unit owner, params Unit[] targets)
    {
        return new QueuedEffect<StartEvent>
        {
            Effect = new Damage<StartEvent> { Value = Literal.Of(1) },
            Event = new StartEvent(),
            Context = new Context(board, owner),
            Targets = targets
        };
    }

    [Fact]
    public void Apply_DeadTargetAmongLiving_SkipsOnlyTheDead()
    {
        Board board = Boards.ThreeVersusThree();
        Unit owner = Boards.Find(board, 0);
        Unit dead = Boards.Find(board, 1);
        Unit living = Boards.Find(board, 3);
        dead.Dead = true;

        IReadOnlyList<BattleEvent> events = Damaging(board, owner, dead, living).Apply();

        Assert.Equal(1, dead.Health);
        Assert.Equal(0, living.Health);
        Assert.Equal([new UnitHurtEvent { Source = owner, Target = living, Value = 1 }], events);
    }

    [Fact]
    public void Apply_AllTargetsDead_EmitsNothing()
    {
        Board board = Boards.ThreeVersusThree();
        Unit owner = Boards.Find(board, 0);
        Unit dead = Boards.Find(board, 1);
        dead.Dead = true;

        IReadOnlyList<BattleEvent> events = Damaging(board, owner, dead).Apply();

        Assert.Equal(1, dead.Health);
        Assert.Empty(events);
    }
}
