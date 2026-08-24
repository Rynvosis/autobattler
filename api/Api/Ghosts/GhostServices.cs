using Api.Storage;

namespace Api.Ghosts;

public static class GhostServices
{
    public static IServiceCollection AddGhosts(this IServiceCollection services)
    {
        services.AddSingleton<GhostStore>();
        services.AddSingleton(new TableDefinition
        {
            CreateTableRequest = GhostTable.CreateRequest(),
            TimeToLiveAttribute = GhostTable.ExpiresAt
        });

        return services;
    }
}
