using Api.Storage;

namespace Api.Runs;

public static class RunServices
{
    public static IServiceCollection AddRuns(this IServiceCollection services)
    {
        services.AddSingleton<RunStore>();
        services.AddSingleton(new TableDefinition
        {
            CreateTableRequest = RunTable.CreateRequest(),
            TimeToLiveAttribute = RunTable.ExpiresAt
        });

        return services;
    }
}
