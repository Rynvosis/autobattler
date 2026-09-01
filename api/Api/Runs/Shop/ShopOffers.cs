using Amazon.DynamoDBv2.Model;
using Api.Content;
using Api.Teams;

namespace Api.Runs.Shop;

public static class ShopOffers
{
    public static IReadOnlyList<TeamUnit?> Roll()
    {
        return [.. Random.Shared.GetItems(Monsters.Pool, Economy.ShopSize).Select(TeamUnits.From)];
    }

    public static AttributeValue ToItems(IReadOnlyList<TeamUnit?> shop)
    {
        return new AttributeValue
        {
            L =
            [
                .. shop.Select(unit => unit is null
                    ? new AttributeValue { NULL = true }
                    : TeamUnits.ToItem(unit))
            ]
        };
    }

    public static IReadOnlyList<TeamUnit?> FromItems(AttributeValue value)
    {
        return
        [
            .. value.L.Select(unit => unit.NULL == true ? null : TeamUnits.FromItem(unit))
        ];
    }
}
