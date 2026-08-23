using Api.Combat.Abilities;
using Api.Combat.Abilities.Scopes;
using Api.Combat.Effects;

namespace Api.Combat.Tests;

public class TriggerScanTests
{
    private static Ability Retaliate() =>
        new()
        {
            Trigger = new TargetTrigger<UnitHurtEvent> { Scopes = [new SelfScope()] },
            Effect = new Damage { Value = 1 },
            Scopes = [new EventSourceScope()]
        };

    private static Board BoardWith(Ability ability, int ownerId) =>
        new(
            new Team([Boards.Unit(0, ownerId == 0 ? ability : null), Boards.Unit(2)]),
            new Team([Boards.Unit(1), Boards.Unit(3)]));

    [Fact]
    public void Scan_MatchingTrigger_QueuesEffectAtTheAbilityOwner()
    {
        Board board = BoardWith(Retaliate(), 0);
        Unit owner = Boards.Find(board, 0);
        Unit dealer = Boards.Find(board, 1);

        IReadOnlyList<QueuedEffect> queued =
            TriggerScan.Scan(board, new UnitHurtEvent { Source = dealer, Target = owner, Value = 1 });

        QueuedEffect only = Assert.Single(queued);
        Assert.Equal(owner, only.Source);
        Assert.Equal([dealer], only.Targets);
    }

    [Fact]
    public void Scan_TriggerOnAnotherUnit_QueuesNothing()
    {
        Board board = BoardWith(Retaliate(), 0);
        Unit other = Boards.Find(board, 2);
        Unit dealer = Boards.Find(board, 1);

        IReadOnlyList<QueuedEffect> queued =
            TriggerScan.Scan(board, new UnitHurtEvent { Source = dealer, Target = other, Value = 1 });

        Assert.Empty(queued);
    }

    [Fact]
    public void Scan_OwnerDiedThisSubtick_StillQueuesItsEffect()
    {
        Board board = BoardWith(Retaliate(), 0);
        Unit owner = Boards.Find(board, 0);
        Unit dealer = Boards.Find(board, 1);
        owner.Dead = true;

        IReadOnlyList<QueuedEffect> queued =
            TriggerScan.Scan(board, new UnitHurtEvent { Source = dealer, Target = owner, Value = 1 });

        QueuedEffect only = Assert.Single(queued);
        Assert.Equal(owner, only.Source);
        Assert.Equal([dealer], only.Targets);
    }

    [Fact]
    public void Scan_DeadTargetInScope_ResolvesToNoTargets()
    {
        Board board = BoardWith(Retaliate(), 0);
        Unit owner = Boards.Find(board, 0);
        Unit dealer = Boards.Find(board, 1);
        dealer.Dead = true;

        IReadOnlyList<QueuedEffect> queued =
            TriggerScan.Scan(board, new UnitHurtEvent { Source = dealer, Target = owner, Value = 1 });

        QueuedEffect only = Assert.Single(queued);
        Assert.Empty(only.Targets);
    }
}
