using Api.Combat.Units;

namespace Api.Content;

public sealed record ContentResponse
{
    public required string Version { get; init; }
    public required IReadOnlyList<ContentUnit> Units { get; init; }
}

public sealed record ContentUnit
{
    public required Kind Kind { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required int Attack { get; init; }
    public required int Health { get; init; }
    public required int Tier { get; init; }
    public required string Description { get; init; }
}
