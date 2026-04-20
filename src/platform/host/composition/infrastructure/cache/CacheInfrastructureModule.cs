using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Cache;

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

        // R2.A.D.3d / R-REDIS-BREAKER-01: shared "redis" breaker. Protects
        // both consumers (RedisExecutionLockProvider command-dispatch lock
        // + RedisHealthSnapshotProvider probe). Registered as plain
        // ICircuitBreaker so CircuitBreakerRegistry picks it up alongside
        // OPA / Chain / Kafka. Defaults: 5 failures / 30s window — tune
        // via Redis:BreakerThreshold / Redis:BreakerWindowSeconds.
        var redisBreakerThreshold = configuration.GetValue<int?>("Redis:BreakerThreshold") ?? 5;
        var redisBreakerWindowSeconds = configuration.GetValue<int?>("Redis:BreakerWindowSeconds") ?? 30;
        services.AddSingleton<ICircuitBreaker>(sp =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions
                {
                    Name = "redis",
                    FailureThreshold = redisBreakerThreshold,
                    WindowSeconds = redisBreakerWindowSeconds
                },
                sp.GetRequiredService<IClock>()));

        // phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
        // distributed execution lock backed by the same Redis
        // singleton already registered above. Singleton-scoped so
        // the per-process owner-token map persists across requests.
        //
        // R2.A.D.3d: resolves its ICircuitBreaker via the registry so
        // Redis outages fast-fail without touching Redis (see
        // R-REDIS-BREAKER-OPEN-FAIL-CLOSED-01).
        services.AddSingleton<Whycespace.Shared.Contracts.Runtime.IExecutionLockProvider>(sp =>
            new Whycespace.Platform.Host.Runtime.RedisExecutionLockProvider(
                sp.GetRequiredService<IConnectionMultiplexer>(),
                sp.GetRequiredService<ICircuitBreakerRegistry>().Get("redis")));

        return services;
    }
}
