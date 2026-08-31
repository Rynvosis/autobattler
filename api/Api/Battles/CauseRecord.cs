using System.Text.Json.Serialization;
using Api.Combat.Events;

namespace Api.Battles;

public readonly record struct CauseRecord
{
    public required CauseKind Kind { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Unit { get; init; }
}