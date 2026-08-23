using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;

namespace Api.Combat.Tests;

public class TriggerTests
{
    public static TheoryData<Trigger, int, Func<Board, BattleEvent>, bool> Cases => new()
    {
        { new RoundTrigger<StartEvent>(), 0, _ => new StartEvent(), true },
        { new RoundTrigger<StartEvent>(), 0, board => Hurt(board, 0), false },
        {
            new TargetTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0, board => Hurt(board, 0), true
        },
        {
            new TargetTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0, board => Hurt(board, 2), false
        },
        {
            new TargetTrigger<UnitHurtEvent>
            {
                Scopes = [new HeadScope { Side = ScopeSide.Ally, Range = ScopeRange.From(0) }]
            },
            0, board => Hurt(board, 2), true
        },
        {
            new TargetTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            0,
            board => new UnitAttackEvent { Source = Boards.Find(board, 1), Target = Boards.Find(board, 0), Value = 1 },
            false
        },
        {
            new SourceTrigger<UnitAttackEvent> { Scopes = [new SelfScope()] },
            0,
            board => new UnitAttackEvent { Source = Boards.Find(board, 0), Target = Boards.Find(board, 1), Value = 1 },
            true
        }
    };

    private static BattleEvent Hurt(Board board, int targetId)
    {
        return new UnitHurtEvent { Source = Boards.Find(board, 1), Target = Boards.Find(board, targetId), Value = 1 };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Matches_ReturnsExpected(Trigger trigger, int ownerId, Func<Board, BattleEvent> makeEvent, bool expected)
    {
        Board board = Boards.ThreeVersusThree();
        TriggerContext context = new(board, Boards.Find(board, ownerId));

        bool matched = trigger.Matches(makeEvent(board), context);

        Assert.Equal(expected, matched);
    }
}
