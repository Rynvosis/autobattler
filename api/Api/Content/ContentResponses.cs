namespace Api.Content;

// The ability tree is engine types with no wire shape, so the manifest is projected rather than returned.
public static class ContentResponses
{
    public static ContentResponse From(ContentManifest manifest)
    {
        return new ContentResponse
        {
            Version = manifest.Version,
            Units =
            [
                .. manifest.Units.Select(definition => new ContentUnit
                {
                    Kind = definition.Kind,
                    Name = definition.Name,
                    Icon = definition.Icon,
                    Attack = definition.Attack,
                    Health = definition.Health,
                    Tier = definition.Tier,
                    Description = definition.Description
                })
            ]
        };
    }
}
