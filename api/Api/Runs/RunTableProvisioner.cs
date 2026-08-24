using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Api.Runs;

public static class RunTableProvisioner
{
    public static async Task EnsureCreatedAsync(IAmazonDynamoDB dynamoDB, CancellationToken cancellationToken = default)
    {
        try
        {
            await dynamoDB.DescribeTableAsync(RunTable.TableName, cancellationToken);
        }
        catch (ResourceNotFoundException)
        {
            await dynamoDB.CreateTableAsync(new CreateTableRequest
            {
                TableName = RunTable.TableName,
                AttributeDefinitions =
                [
                    new AttributeDefinition
                    {
                        AttributeName = RunTable.RunId,
                        AttributeType = ScalarAttributeType.S
                    }
                ],
                KeySchema =
                [
                    new KeySchemaElement
                    {
                        AttributeName = RunTable.RunId,
                        KeyType = KeyType.HASH
                    }
                ],
                BillingMode = BillingMode.PAY_PER_REQUEST
            }, cancellationToken);

            await dynamoDB.UpdateTimeToLiveAsync(new UpdateTimeToLiveRequest
            {
                TableName = RunTable.TableName,
                TimeToLiveSpecification = new TimeToLiveSpecification
                {
                    Enabled = true,
                    AttributeName = RunTable.ExpiresAt
                }
            }, cancellationToken);
        }
    }
}