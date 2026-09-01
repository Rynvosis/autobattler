using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.DynamoDb;

namespace Api.Tests;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly DynamoDbContainer _container = new DynamoDbBuilder("amazon/dynamodb-local:latest").Build();

    public WebApplicationFactory<Program> Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Api = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
        {
            host.UseSetting("AWS:Region", "eu-west-2");
            host.UseSetting("DynamoDB:ServiceUrl", _container.GetConnectionString());
        });
    }

    public async Task DisposeAsync()
    {
        await Api.DisposeAsync();
        await _container.DisposeAsync();
    }
}
