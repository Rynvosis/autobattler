using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Combat.Units;

namespace Api.Serialization;

// Kind is a record struct, so its default shape is {"value":"golem"} rather than "golem".
public sealed class KindJsonConverter : JsonConverter<Kind>
{
    public override Kind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new Kind(reader.GetString() ?? throw new JsonException($"A kind is not a JSON {reader.TokenType}"));
    }

    public override void Write(Utf8JsonWriter writer, Kind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
