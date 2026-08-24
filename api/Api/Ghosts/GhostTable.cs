using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Api.Ghosts;

public static class GhostTable
{
    public const string TableName = "ghosts";

    public const string Stage = "stage";
    public const string RunId = "runId";
    public const string ExpiresAt = "expiresAt";
    public const string Units = "units";

    public static CreateTableRequest CreateRequest() => new()
    {
        TableName = TableName,
        AttributeDefinitions =
        [
            new AttributeDefinition
            {
                AttributeName = Stage,
                AttributeType = ScalarAttributeType.N
            },
            new AttributeDefinition
            {
                AttributeName = RunId,
                AttributeType = ScalarAttributeType.S
            }
        ],
        KeySchema =
        [
            new KeySchemaElement
            {
                AttributeName = Stage,
                KeyType = KeyType.HASH
            },
            new KeySchemaElement
            {
                AttributeName = RunId,
                KeyType = KeyType.RANGE
            }
        ],
        BillingMode = BillingMode.PAY_PER_REQUEST
    };
}
