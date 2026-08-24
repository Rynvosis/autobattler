using Amazon.DynamoDBv2.Model;

namespace Api.Storage;

public sealed record TableDefinition
{
    public required CreateTableRequest CreateTableRequest { get; init; }
    public required string TimeToLiveAttribute { get; init; }
}
