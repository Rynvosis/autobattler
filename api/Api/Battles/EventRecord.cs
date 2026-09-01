using System.Text.Json.Serialization;

namespace Api.Battles;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StartRecord), "start")]
[JsonDerivedType(typeof(UnitAttackRecord), "unitAttack")]
[JsonDerivedType(typeof(UnitHurtRecord), "unitHurt")]
[JsonDerivedType(typeof(UnitAttackChangeRecord), "unitAttackChange")]
[JsonDerivedType(typeof(UnitHealthChangeRecord), "unitHealthChange")]
[JsonDerivedType(typeof(UnitDeathRecord), "unitDeath")]
[JsonDerivedType(typeof(UnitKillRecord), "unitKill")]
public abstract record EventRecord
{
    public int Tick { get; init; }
    public int Subtick { get; init; }
    public CauseRecord Cause { get; init; }
}

public abstract record TargetedRecord : EventRecord
{
    public required int Target { get; init; }
}

public abstract record SourcedRecord : TargetedRecord
{
    public required int Source { get; init; }
}

public abstract record ValuedRecord : SourcedRecord
{
    public required int Value { get; init; }
}

public sealed record StartRecord : EventRecord;

public sealed record UnitDeathRecord : TargetedRecord;

public sealed record UnitKillRecord : SourcedRecord;

public sealed record UnitAttackRecord : ValuedRecord;

public sealed record UnitHurtRecord : ValuedRecord;

public sealed record UnitAttackChangeRecord : ValuedRecord;

public sealed record UnitHealthChangeRecord : ValuedRecord;
