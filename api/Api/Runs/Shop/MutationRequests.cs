namespace Api.Runs.Shop;

public record MutationRequest
{
    public required int Version { get; init; }
}

public sealed record ShopSlotRequest : MutationRequest
{
    public required int ShopSlot { get; init; }
}

public sealed record TeamSlotRequest : MutationRequest
{
    public required int TeamSlot { get; init; }
}

public sealed record ReorderRequest : MutationRequest
{
    public required IReadOnlyList<int> Order { get; init; }
}
