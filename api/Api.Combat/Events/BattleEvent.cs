namespace Api.Combat.Events;

public record BattleEvent
{
    public required EventKind Kind { get; init; }
    public required int Tick { get; init; }
    public required int Subtick { get; init; }

    public int? Source { get; init; }
    public int? Target { get; init; }

    public int? Value { get; init; }
}
