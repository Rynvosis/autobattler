using Amazon.DynamoDBv2.Model;
using Api.Combat.Battlefield;
using Api.Combat.Units;
using Api.Content;
using Api.Storage;

namespace Api.Teams;

public static class TeamUnits
{
    public static AttributeValue ToItems(IReadOnlyList<TeamUnit> units)
    {
        return new AttributeValue { L = [.. units.Select(ToItem)] };
    }

    public static IReadOnlyList<TeamUnit> FromItems(AttributeValue value)
    {
        return [.. value.L.Select(FromItem)];
    }

    public static AttributeValue ToItem(TeamUnit unit)
    {
        return new AttributeValue
        {
            M = new Dictionary<string, AttributeValue>
            {
                [Attributes.Kind] = new() { S = unit.Kind.Value },
                [Attributes.Attack] = AttributeValues.Number(unit.Attack),
                [Attributes.Health] = AttributeValues.Number(unit.Health)
            }
        };
    }

    public static TeamUnit FromItem(AttributeValue unit)
    {
        return new TeamUnit
        {
            Kind = new Kind(unit.M[Attributes.Kind].S),
            Attack = AttributeValues.ToInt32(unit.M[Attributes.Attack]),
            Health = AttributeValues.ToInt32(unit.M[Attributes.Health])
        };
    }

    // Combat mutates the units it is given, and unit ids are disjoint across both teams.
    public static Team ToTeam(IReadOnlyList<TeamUnit> units, int firstId) =>
        new([
            .. units.Select((unit, slot) => new Unit
            {
                Id = firstId + slot,
                Kind = unit.Kind,
                Attack = unit.Attack,
                Health = unit.Health
            })
        ]);

    public static TeamUnit From(UnitDefinition definition)
    {
        return new TeamUnit
        {
            Kind = definition.Kind,
            Attack = definition.Attack,
            Health = definition.Health
        };
    }

    private static class Attributes
    {
        public const string Kind = "kind";
        public const string Attack = "attack";
        public const string Health = "health";
    }
}
