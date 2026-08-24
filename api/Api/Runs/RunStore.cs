using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Api.Combat.Units;

namespace Api.Runs;

public sealed class RunStore(IAmazonDynamoDB dynamoDB)
{
    public async Task<Run?> GetAsync(string runId, CancellationToken cancellationToken)
    {
        GetItemResponse response = await dynamoDB.GetItemAsync(new GetItemRequest
        {
            TableName = RunTable.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                [RunTable.RunId] = new() { S = runId }
            },
            ConsistentRead = true
        }, cancellationToken);

        return response.IsItemSet ? FromItem(response.Item) : null;
    }

    public Task PutAsync(Run run, CancellationToken cancellationToken)
    {
        return dynamoDB.PutItemAsync(new PutItemRequest
        {
            TableName = RunTable.TableName,
            Item = ToItem(run)
        }, cancellationToken);
    }

    private static Dictionary<string, AttributeValue> ToItem(Run run)
    {
        return new Dictionary<string, AttributeValue>
        {
            [RunTable.RunId] = new() { S = run.RunId },
            [RunTable.Version] = ToNumber(run.Version),
            [RunTable.Gold] = ToNumber(run.Gold),
            [RunTable.Tier] = ToNumber(run.Tier),
            [RunTable.ExpiresAt] = ToNumber(run.ExpiresAt.ToUnixTimeSeconds()),
            [RunTable.Units] = new()
            {
                L =
                [
                    .. run.Units.Select(unit => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            [RunTable.Unit.Kind] = new() { S = unit.Kind.Value },
                            [RunTable.Unit.Attack] = ToNumber(unit.Attack),
                            [RunTable.Unit.Health] = ToNumber(unit.Health)
                        }
                    })
                ]
            }
        };
    }

    private static Run FromItem(Dictionary<string, AttributeValue> item)
    {
        return new Run
        {
            RunId = item[RunTable.RunId].S,
            Version = ToInt32(item[RunTable.Version]),
            Gold = ToInt32(item[RunTable.Gold]),
            Tier = ToInt32(item[RunTable.Tier]),
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(ToInt64(item[RunTable.ExpiresAt])),
            Units =
            [
                .. item[RunTable.Units].L.Select(unit => new RunUnit
                {
                    Kind = new Kind(unit.M[RunTable.Unit.Kind].S),
                    Attack = ToInt32(unit.M[RunTable.Unit.Attack]),
                    Health = ToInt32(unit.M[RunTable.Unit.Health])
                })
            ]
        };
    }

    private static AttributeValue ToNumber(long value)
    {
        return new AttributeValue { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    private static int ToInt32(AttributeValue value)
    {
        return int.Parse(value.N, CultureInfo.InvariantCulture);
    }

    private static long ToInt64(AttributeValue value)
    {
        return long.Parse(value.N, CultureInfo.InvariantCulture);
    }
}