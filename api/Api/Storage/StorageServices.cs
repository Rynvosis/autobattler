using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;

namespace Api.Storage;

public static class StorageServices
{
    private const string ServiceUrlKey = "DynamoDB:ServiceUrl";

    public static IServiceCollection AddDynamoDb(this IServiceCollection services, IConfiguration configuration)
    {
        AWSOptions options = configuration.GetAWSOptions();
        string? serviceUrl = configuration[ServiceUrlKey];

        if (!string.IsNullOrEmpty(serviceUrl))
        {
            options.DefaultClientConfig.ServiceURL = serviceUrl;
            options.Credentials = new BasicAWSCredentials("accessKey", "secretKey");
        }

        services.AddDefaultAWSOptions(options);
        services.AddAWSService<IAmazonDynamoDB>();

        return services;
    }

    public static async Task EnsureLocalTablesCreatedAsync(this WebApplication app)
    {
        if (string.IsNullOrEmpty(app.Configuration[ServiceUrlKey])) return;

        IAmazonDynamoDB dynamoDB = app.Services.GetRequiredService<IAmazonDynamoDB>();
        CancellationToken cancellationToken = app.Lifetime.ApplicationStopping;

        foreach (TableDefinition table in app.Services.GetServices<TableDefinition>())
        {
            await TableProvisioner.EnsureCreatedAsync(dynamoDB, table, cancellationToken);
        }
    }
}
