using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Api.Storage;
using Api.Teams;

namespace Api.Ghosts;

public sealed class GhostStore(IAmazonDynamoDB dynamoDB)
{
    public Task PutAsync(Ghost ghost, CancellationToken cancellationToken)
    {
        return dynamoDB.PutItemAsync(new PutItemRequest
        {
            TableName = GhostTable.TableName,
            Item = ToItem(ghost)
        }, cancellationToken);
    }

    // TODO: choose among the ghosts on the stage, excluding the run's own.
    public Task<Ghost?> FindOpponentAsync(int stage, string excludingRunId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    private static Dictionary<string, AttributeValue> ToItem(Ghost ghost)
    {
        return new Dictionary<string, AttributeValue>
        {
            [GhostTable.Stage] = AttributeValues.Number(ghost.Stage),
            [GhostTable.RunId] = new AttributeValue { S = ghost.RunId },
            [GhostTable.ExpiresAt] = AttributeValues.Number(ghost.ExpiresAt.ToUnixTimeSeconds()),
            [GhostTable.Units] = TeamUnits.ToItem(ghost.Units)
        };
    }

    private static Ghost FromItem(Dictionary<string, AttributeValue> item)
    {
        return new Ghost
        {
            Stage = AttributeValues.ToInt32(item[GhostTable.Stage]),
            RunId = item[GhostTable.RunId].S,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(AttributeValues.ToInt64(item[GhostTable.ExpiresAt])),
            Units = TeamUnits.FromItem(item[GhostTable.Units])
        };
    }
}
