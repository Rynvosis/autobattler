using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Api.Storage;
using Api.Teams;

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
                [RunTable.RunId] = new AttributeValue { S = runId }
            },
            ConsistentRead = true
        }, cancellationToken);

        return response.IsItemSet ? FromItem(response.Item) : null;
    }

    // TODO: condition the write on the stored version.
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
            [RunTable.RunId] = new AttributeValue { S = run.RunId },
            [RunTable.Version] = AttributeValues.Number(run.Version),
            [RunTable.Victories] = AttributeValues.Number(run.Victories),
            [RunTable.Gold] = AttributeValues.Number(run.Gold),
            [RunTable.Stage] = AttributeValues.Number(run.Stage),
            [RunTable.ExpiresAt] = AttributeValues.Number(run.ExpiresAt.ToUnixTimeSeconds()),
            [RunTable.Units] = TeamUnits.ToItem(run.Units)
        };
    }

    private static Run FromItem(Dictionary<string, AttributeValue> item)
    {
        return new Run
        {
            RunId = item[RunTable.RunId].S,
            Version = AttributeValues.ToInt32(item[RunTable.Version]),
            Victories = AttributeValues.ToInt32(item[RunTable.Victories]),
            Gold = AttributeValues.ToInt32(item[RunTable.Gold]),
            Stage = AttributeValues.ToInt32(item[RunTable.Stage]),
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(AttributeValues.ToInt64(item[RunTable.ExpiresAt])),
            Units = TeamUnits.FromItem(item[RunTable.Units])
        };
    }
}