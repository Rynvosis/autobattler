using Api.Combat.Abilities;
using Api.Combat.Units;

namespace Api.Content;

public static class ContentManifestExtensions
{
    public static Roster ToRoster(this ContentManifest manifest)
    {
        Dictionary<Kind, Ability> abilities = [];

        foreach (UnitDefinition definition in manifest.Units)
            if (definition.Ability is { } ability)
                abilities.Add(definition.Kind, ability);

        return new Roster { Abilities = abilities };
    }
}
