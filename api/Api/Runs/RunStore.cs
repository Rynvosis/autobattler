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

    public async Task<Run> CreateAsync(Run run, CancellationToken cancellationToken)
    {
        Run created = run with { Version = 1 };

        await dynamoDB.PutItemAsync(new PutItemRequest
        {
            TableName = RunTable.TableName,
            Item = ToItem(created),
            ConditionExpression = $"attribute_not_exists({RunTable.RunId})"
        }, cancellationToken);

        return created;
    }

    public async Task<Run> UpdateAsync(Run run, CancellationToken cancellationToken)
    {
        Run updated = run with { Version = run.Version + 1 };

        try
        {
            await dynamoDB.PutItemAsync(new PutItemRequest
            {
                TableName = RunTable.TableName,
                Item = ToItem(updated),
                ConditionExpression = "#version = :expectedVersion",
                // VERSION is a DynamoDB reserved word, hence the alias.
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#version"] = RunTable.Version
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":expectedVersion"] = AttributeValues.Number(run.Version)
                },
                ReturnValuesOnConditionCheckFailure = ReturnValuesOnConditionCheckFailure.ALL_OLD
            }, cancellationToken);
        }
        catch (ConditionalCheckFailedException conflict)
        {
            throw new RunConflictException(FromItem(conflict.Item));
        }

        return updated;
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