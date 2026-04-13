using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Whyce.Platform.Host.Adapters;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Projection;

namespace Whyce.Platform.Host.Composition.Infrastructure.Cache;

/// <summary>
/// Cache capability — Redis connection, client, and distributed execution lock.
/// </summary>
public static class CacheInfrastructureModule
{
    public static IServiceCollection AddCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString")
            ?? throw new InvalidOperationException("Redis:ConnectionString is required. No fallback.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IRedisClient>(sp =>
            new StackExchangeRedisClient(sp.GetRequiredService<IConnectionMultiplexer>()));
        // phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
        // distributed execution lock backed by the same Redis
        // singleton already registered above. Singleton-scoped so
        // the per-process owner-token map persists across requests.
        services.AddSingleton<Whyce.Shared.Contracts.Runtime.IExecutionLockProvider>(sp =>
            new Whyce.Platform.Host.Runtime.RedisExecutionLockProvider(
                sp.GetRequiredService<IConnectionMultiplexer>()));

        return services;
    }
}
