using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.DynamoDb;

namespace Api.Tests;

// The app creates its own tables against the container, so a test exercises the wiring it ships.
public abstract class ApiTests : IAsyncLifetime
{
    private readonly DynamoDbContainer _container = new DynamoDbBuilder("amazon/dynamodb-local:latest").Build();

    private WebApplicationFactory<Program> _api = null!;

    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _api = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
        {
            host.UseSetting("AWS:Region", "eu-west-2");
            host.UseSetting("DynamoDB:ServiceUrl", _container.GetConnectionString());
        });

        Client = _api.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _api.DisposeAsync();
        await _container.DisposeAsync();
    }

    protected T Service<T>() where T : notnull
    {
        return _api.Services.GetRequiredService<T>();
    }
}