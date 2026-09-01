using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Api.Storage;
using Api.Teams;

namespace Api.Ghosts;

public sealed class GhostStore(IAmazonDynamoDB dynamoDB)
{
    private const int PageSize = 20;

    public Task PutAsync(Ghost ghost, CancellationToken cancellationToken)
    {
        return dynamoDB.PutItemAsync(new PutItemRequest
        {
            TableName = GhostTable.TableName,
            Item = ToItem(ghost)
        }, cancellationToken);
    }

    // The run's own ghost is dropped here because DynamoDB rejects key attributes in a filter.
    public async Task<IReadOnlyList<Ghost>> FindOpponentsAsync(
        int stage,
        string excludingRunId,
        CancellationToken cancellationToken)
    {
        QueryResponse response = await dynamoDB.QueryAsync(new QueryRequest
        {
            TableName = GhostTable.TableName,
            KeyConditionExpression = "#stage = :stage",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#stage"] = GhostTable.Stage
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":stage"] = AttributeValues.Number(stage)
            },
            Limit = PageSize
        }, cancellationToken);

        return [.. response.Items.Select(FromItem).Where(ghost => ghost.RunId != excludingRunId)];
    }

    private static Dictionary<string, AttributeValue> ToItem(Ghost ghost)
    {
        return new Dictionary<string, AttributeValue>
        {
            [GhostTable.Stage] = AttributeValues.Number(ghost.Stage),
            [GhostTable.RunId] = new AttributeValue { S = ghost.RunId },
            [GhostTable.ExpiresAt] = AttributeValues.Number(ghost.ExpiresAt.ToUnixTimeSeconds()),
            [GhostTable.Units] = TeamUnits.ToItems(ghost.Units)
        };
    }

    private static Ghost FromItem(Dictionary<string, AttributeValue> item)
    {
        return new Ghost
        {
            Stage = AttributeValues.ToInt32(item[GhostTable.Stage]),
            RunId = item[GhostTable.RunId].S,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(AttributeValues.ToInt64(item[GhostTable.ExpiresAt])),
            Units = TeamUnits.FromItems(item[GhostTable.Units])
        };
    }
}