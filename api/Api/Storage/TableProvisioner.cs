using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Api.Storage;

public static class TableProvisioner
{
    public static async Task EnsureCreatedAsync(
        IAmazonDynamoDB dynamoDB,
        TableDefinition table,
        CancellationToken cancellationToken)
    {
        try
        {
            await dynamoDB.DescribeTableAsync(table.CreateTableRequest.TableName, cancellationToken);
        }
        catch (ResourceNotFoundException)
        {
            await dynamoDB.CreateTableAsync(table.CreateTableRequest, cancellationToken);

            await dynamoDB.UpdateTimeToLiveAsync(new UpdateTimeToLiveRequest
            {
                TableName = table.CreateTableRequest.TableName,
                TimeToLiveSpecification = new TimeToLiveSpecification
                {
                    Enabled = true,
                    AttributeName = table.TimeToLiveAttribute
                }
            }, cancellationToken);
        }
    }
}
