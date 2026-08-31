using System.Text.Json;
using Amazon.DynamoDBv2;
using Api.Storage;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Api.Tests;

// The app creates its own tables against the container, so a test exercises the wiring it ships.
public abstract class ApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>, IAsyncLifetime
{
    protected HttpClient Client { get; private set; } = null!;

    protected JsonSerializerOptions Json => Service<IOptions<JsonOptions>>().Value.SerializerOptions;

    public async Task InitializeAsync()
    {
        await ResetTablesAsync();

        Client = fixture.Api.CreateClient();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();

        return Task.CompletedTask;
    }

    protected T Service<T>() where T : notnull
    {
        return fixture.Api.Services.GetRequiredService<T>();
    }

    // The container is shared by every test in the class, so each one starts from empty tables.
    private async Task ResetTablesAsync()
    {
        IAmazonDynamoDB dynamoDB = Service<IAmazonDynamoDB>();

        foreach (TableDefinition table in fixture.Api.Services.GetServices<TableDefinition>())
        {
            await dynamoDB.DeleteTableAsync(table.CreateTableRequest.TableName);
            await TableProvisioner.EnsureCreatedAsync(dynamoDB, table, CancellationToken.None);
        }
    }
}