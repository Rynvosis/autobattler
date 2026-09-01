using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Api.Runs;

public static class RunTable
{
    public const string TableName = "runs";

    public const string RunId = "runId";
    public const string Version = "version";
    public const string Victories = "victories";
    public const string Gold = "gold";
    public const string Stage = "stage";
    public const string ExpiresAt = "expiresAt";
    public const string Units = "units";
    public const string Shop = "shop";
    public const string Duplicated = "duplicated";
    public const string UpgradeCredits = "upgradeCredits";

    public static CreateTableRequest CreateRequest() => new()
    {
        TableName = TableName,
        AttributeDefinitions =
        [
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
                AttributeName = RunId,
                KeyType = KeyType.HASH
            }
        ],
        BillingMode = BillingMode.PAY_PER_REQUEST
    };
}
