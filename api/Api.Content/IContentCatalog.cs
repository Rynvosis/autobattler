using Api.Combat.Units;

namespace Api.Content;

public interface IContentCatalog
{
    ContentManifest Manifest { get; }

    UnitDefinition Definition(Kind kind);
}
