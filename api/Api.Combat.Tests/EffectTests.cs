using Api.Combat.Effects;
using Api.Combat.Events;

namespace Api.Combat.Tests;

public class EffectTests
{
    private sealed class RecordingContext : IResolutionContext
    {
        public List<BattleEvent> Events { get; } = [];

        public void Emit(BattleEvent battleEvent) => Events.Add(battleEvent);
    }

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
}
