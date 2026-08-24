using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;

namespace Api.Runs;

public static class RunsExtensions
{
    private const string ServiceUrlKey = "DynamoDB:ServiceUrl";

    public static IServiceCollection AddRuns(this IServiceCollection services, IConfiguration configuration)
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
        services.AddSingleton<RunStore>();

        return services;
    }

    public static async Task EnsureRunStorageAsync(this WebApplication app)
    {
        if (string.IsNullOrEmpty(app.Configuration[ServiceUrlKey])) return;

        await RunTableProvisioner.EnsureCreatedAsync(app.Services.GetRequiredService<IAmazonDynamoDB>());
    }
}