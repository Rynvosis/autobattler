namespace Api.Content;

public sealed record ContentManifest
{
    public required string Version { get; init; }
    public required IReadOnlyList<UnitDefinition> Units { get; init; }
}
