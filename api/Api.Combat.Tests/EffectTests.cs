using Api.Combat.Effects;

namespace Api.Combat.Tests;

public class EffectTests
{
    [Fact]
    public void Damage_LiveTarget_LowersHealthAndEmitsHurt()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        RecordingContext context = new();

        new Damage { Value = 1 }.Apply(context, source, [target]);

        Assert.Equal(0, target.Health);
        Assert.Equal([new UnitHurtEvent { Source = source, Target = target, Value = 1 }], context.Events);
    }

    [Fact]
    public void Damage_DeadTarget_EmitsNothing()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        target.Dead = true;
        RecordingContext context = new();

        new Damage { Value = 1 }.Apply(context, source, [target]);

        Assert.Equal(1, target.Health);
        Assert.Empty(context.Events);
    }

    [Fact]
    public void Damage_ZeroValue_EmitsNothing()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        RecordingContext context = new();

        new Damage { Value = 0 }.Apply(context, source, [target]);

        Assert.Equal(1, target.Health);
        Assert.Empty(context.Events);
    }

    [Fact]
    public void StatChange_LiveTarget_RaisesBothStatsAndEmitsBothChanges()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        RecordingContext context = new();

        new StatChange { Attack = 1, Health = 2 }.Apply(context, source, [target]);

        Assert.Equal(2, target.Attack);
        Assert.Equal(3, target.Health);
        Assert.Equal(
            [
                new UnitAttackChangeEvent { Source = source, Target = target, Value = 1 },
                new UnitHealthChangeEvent { Source = source, Target = target, Value = 2 }
            ],
            context.Events);
    }

    [Fact]
    public void StatChange_HealthOnly_EmitsNoAttackChange()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        RecordingContext context = new();

        new StatChange { Attack = 0, Health = 1 }.Apply(context, source, [target]);

        Assert.Equal(
            [new UnitHealthChangeEvent { Source = source, Target = target, Value = 1 }],
            context.Events);
    }

    [Fact]
    public void StatChange_NegativeHealth_LeavesTargetForTheDeathPass()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        RecordingContext context = new();

        new StatChange { Attack = 0, Health = -1 }.Apply(context, source, [target]);

        Assert.Equal(0, target.Health);
        Assert.False(target.Dead);
    }

    [Fact]
    public void StatChange_DeadTarget_EmitsNothing()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        target.Dead = true;
        RecordingContext context = new();

        new StatChange { Attack = 1, Health = 1 }.Apply(context, source, [target]);

        Assert.Equal(1, target.Attack);
        Assert.Equal(1, target.Health);
        Assert.Empty(context.Events);
    }

    [Fact]
    public void StatChange_NoDeltas_EmitsNothing()
    {
        Unit source = Boards.Unit(0);
        Unit target = Boards.Unit(1);
        RecordingContext context = new();

        new StatChange { Attack = 0, Health = 0 }.Apply(context, source, [target]);

        Assert.Empty(context.Events);
    }

    private sealed class RecordingContext : IResolutionContext
    {
        public List<BattleEvent> Events { get; } = [];

        public void Emit(BattleEvent battleEvent)
        {
            Events.Add(battleEvent);
        }
    }
}
