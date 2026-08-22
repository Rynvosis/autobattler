namespace Api.Combat;

public record BattleEvent
{
    public required EventKind Kind { get; init; }
    public required int Tick { get; init; }
    public required int Subtick { get; init; }

    public Side? SourceSide { get; init; }
    public int? SourceSlot { get; init; }

    public Side? TargetSide { get; init; }
    public int? TargetSlot { get; init; }

    public int? Value { get; init; }
}
