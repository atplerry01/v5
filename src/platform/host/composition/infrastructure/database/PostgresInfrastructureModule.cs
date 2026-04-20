using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Database;

/// <summary>
/// Postgres capability — connection pools, data sources, event store,
/// idempotency, and sequence store registrations.
/// </summary>
public static class PostgresInfrastructureModule
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- Connection strings (NO hardcoded fallback for required deps) ---
        // phase1.6-CFG-K1: env-var lookup uses Section:Key form. .NET's
        // AddEnvironmentVariables() rewrites env-var "Foo__Bar" to config
        // key "Foo:Bar"; the literal "Foo__Bar" lookup never matches an
        // env var by design. See claude/new-rules/20260408-145000-validation-live-execution.md.
        var postgresEventStoreCs = configuration.GetValue<string>("Postgres:ConnectionString")
            ?? throw new InvalidOperationException("Postgres:ConnectionString is required. No fallback.");
        var postgresChainCs = configuration.GetValue<string>("Postgres:ChainConnectionString")
            ?? postgresEventStoreCs;
        var postgresOutboxCs = postgresEventStoreCs;
        // phase1.5-S5.2.2 / KC-4 (PROJECTIONS-POOL-01): canonical
        // projections connection string. Normalises the pre-KC-4 split
        // between Postgres:ProjectionsConnectionString (read by
        // projection bootstrap) and Projections:ConnectionString (read by
        // TodoController) — Postgres:ProjectionsConnectionString is
        // chosen as the canonical key because it matches the
        // Postgres:ConnectionString / Postgres:ChainConnectionString
        // family. The legacy Projections:ConnectionString key is no
        // longer read by any production code path after KC-4.
        var postgresProjectionsCs = configuration.GetValue<string>("Postgres:ProjectionsConnectionString")
            ?? throw new InvalidOperationException(
                "Postgres:ProjectionsConnectionString is required. No fallback.");

        // --- Postgres logical pools (phase1.5-S5.2.1 / PC-4 POSTGRES-POOL-01) ---
        // Two declared pools sized from configuration: event-store
        // (shared by event store, idempotency, sequence, outbox enqueue,
        // outbox publisher, outbox depth sampler) and chain (used by
        // the chain anchor only). Defaults are baked into
        // PostgresPoolOptions; missing keys produce a real, declared
        // pool rather than the pre-S5.2.1 incidental Npgsql library
        // defaults. Per-command/connection sizing is appended to the
        // base connection string via NpgsqlConnectionStringBuilder so
        // adapters do not need to know the keys exist.
        var poolDefaults = new PostgresPoolOptions();
        var eventStorePoolOptions = new PostgresPoolOptions
        {
            PoolName = EventStoreDataSource.PoolName,
            ConnectionString = postgresEventStoreCs,
            MaxPoolSize = configuration.GetValue<int?>("Postgres:Pools:EventStore:MaxPoolSize")
                ?? poolDefaults.MaxPoolSize,
            MinPoolSize = configuration.GetValue<int?>("Postgres:Pools:EventStore:MinPoolSize")
                ?? poolDefaults.MinPoolSize,
            ConnectionTimeoutSeconds = configuration.GetValue<int?>("Postgres:Pools:EventStore:ConnectionTimeoutSeconds")
                ?? poolDefaults.ConnectionTimeoutSeconds,
            CommandTimeoutSeconds = configuration.GetValue<int?>("Postgres:Pools:EventStore:CommandTimeoutSeconds")
                ?? poolDefaults.CommandTimeoutSeconds,
        };
        var chainPoolOptions = new PostgresPoolOptions
        {
            PoolName = ChainDataSource.PoolName,
            ConnectionString = postgresChainCs,
            MaxPoolSize = configuration.GetValue<int?>("Postgres:Pools:Chain:MaxPoolSize")
                ?? poolDefaults.MaxPoolSize,
            MinPoolSize = configuration.GetValue<int?>("Postgres:Pools:Chain:MinPoolSize")
                ?? poolDefaults.MinPoolSize,
            ConnectionTimeoutSeconds = configuration.GetValue<int?>("Postgres:Pools:Chain:ConnectionTimeoutSeconds")
                ?? poolDefaults.ConnectionTimeoutSeconds,
            CommandTimeoutSeconds = configuration.GetValue<int?>("Postgres:Pools:Chain:CommandTimeoutSeconds")
                ?? poolDefaults.CommandTimeoutSeconds,
        };
        // phase1.5-S5.2.2 / KC-4 (PROJECTIONS-POOL-01): third declared
        // logical pool — projections — backing PostgresProjectionWriter,
        // TodoController.Get, and (via the inner NpgsqlDataSource) the
        // TodoProjectionHandler in the projections assembly. Same
        // BuildDataSource path as EventStore + Chain so pool sizing,
        // timeouts, and validation are uniform across all three.
        var projectionsPoolOptions = new PostgresPoolOptions
        {
            PoolName = ProjectionsDataSource.PoolName,
            ConnectionString = postgresProjectionsCs,
            MaxPoolSize = configuration.GetValue<int?>("Postgres:Pools:Projections:MaxPoolSize")
                ?? poolDefaults.MaxPoolSize,
            MinPoolSize = configuration.GetValue<int?>("Postgres:Pools:Projections:MinPoolSize")
                ?? poolDefaults.MinPoolSize,
            ConnectionTimeoutSeconds = configuration.GetValue<int?>("Postgres:Pools:Projections:ConnectionTimeoutSeconds")
                ?? poolDefaults.ConnectionTimeoutSeconds,
            CommandTimeoutSeconds = configuration.GetValue<int?>("Postgres:Pools:Projections:CommandTimeoutSeconds")
                ?? poolDefaults.CommandTimeoutSeconds,
        };
        // R2.A.D.3c / R-POSTGRES-POOL-BREAKER-01: shared "postgres-pool"
        // breaker. Wraps every pooled connection acquisition across the
        // three declared Npgsql pools (event-store, chain, projections).
        // A single shared breaker is chosen because the three pools
        // typically point at the same Postgres instance in deployments —
        // an outage affects all three uniformly and should produce one
        // coherent health signal. Per-pool TotalFailures (HC-6) remains
        // for diagnostic attribution. Defaults: 5 failures / 30s window —
        // tune via Postgres:BreakerThreshold / Postgres:BreakerWindowSeconds.
        var postgresBreakerThreshold = configuration.GetValue<int?>("Postgres:BreakerThreshold") ?? 5;
        var postgresBreakerWindowSeconds = configuration.GetValue<int?>("Postgres:BreakerWindowSeconds") ?? 30;
        services.AddSingleton<ICircuitBreaker>(sp =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions
                {
                    Name = "postgres-pool",
                    FailureThreshold = postgresBreakerThreshold,
                    WindowSeconds = postgresBreakerWindowSeconds
                },
                sp.GetRequiredService<IClock>()));

        // R2.A.D.3c: the three DataSource wrappers resolve the shared
        // "postgres-pool" breaker via the registry so every adapter
        // acquiring connections through *.OpenAsync(ct) flows through
        // the same breaker gate.
        services.AddSingleton(sp => new EventStoreDataSource(
            BuildDataSource(eventStorePoolOptions),
            sp.GetRequiredService<ICircuitBreakerRegistry>().Get("postgres-pool")));
        services.AddSingleton(sp => new ChainDataSource(
            BuildDataSource(chainPoolOptions),
            sp.GetRequiredService<ICircuitBreakerRegistry>().Get("postgres-pool")));
        services.AddSingleton(sp => new ProjectionsDataSource(
            BuildDataSource(projectionsPoolOptions),
            sp.GetRequiredService<ICircuitBreakerRegistry>().Get("postgres-pool")));

        // R2.A.D.3c: canonical ProjectionStoreFactory carries the shared
        // pool breaker into every PostgresProjectionStore built across the
        // six projection modules. Registering as a singleton here lets each
        // module resolve via DI instead of rewriting 40+ inline factory
        // construction sites with an extra ctor parameter.
        services.AddSingleton(sp => new Whycespace.Projections.Shared.ProjectionStoreFactory(
            sp.GetRequiredService<ProjectionsDataSource>().Inner,
            sp.GetRequiredService<ICircuitBreakerRegistry>().Get("postgres-pool")));

        // phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): canonical
        // catalog of declared pool name → MaxPoolSize. Read by
        // PostgresPoolSnapshotProvider so the HC-6 snapshot's
        // MaxConnections field reflects the declared envelope rather
        // than a guess. The catalog must be derived from the same
        // PostgresPoolOptions instances used to build the data
        // sources above so the two cannot drift.
        services.AddSingleton(new Whycespace.Platform.Host.Health.PostgresPoolCatalog(
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [eventStorePoolOptions.PoolName] = eventStorePoolOptions.MaxPoolSize,
                [chainPoolOptions.PoolName] = chainPoolOptions.MaxPoolSize,
                [projectionsPoolOptions.PoolName] = projectionsPoolOptions.MaxPoolSize,
            }));

        // --- Event store + schema-driven deserializer ---
        services.AddSingleton<IEventStore>(sp =>
            new PostgresEventStoreAdapter(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<IIdGenerator>()));
        services.AddSingleton<EventDeserializer>();

        // --- Idempotency, sequence ---
        services.AddSingleton<IIdempotencyStore>(sp =>
            new PostgresIdempotencyStoreAdapter(sp.GetRequiredService<EventStoreDataSource>()));
        services.AddSingleton<ISequenceStore>(sp =>
            new PostgresSequenceStoreAdapter(sp.GetRequiredService<EventStoreDataSource>()));

        // R2.A.3b / R-DLQ-STORE-01: durable dead-letter mirror. Wired into
        // KafkaOutboxPublisher (infrastructure/messaging module) so every
        // message routed to a `.deadletter` Kafka topic is also recorded
        // here for operator inspection / re-drive / retention.
        services.AddSingleton<Whycespace.Shared.Contracts.Infrastructure.Messaging.IDeadLetterStore>(sp =>
            new PostgresDeadLetterStore(sp.GetRequiredService<EventStoreDataSource>()));

        // R2.A.C.1 / R-LEASE-POSTGRES-01: D3 LOCKED → Postgres advisory
        // lock based distributed leases. Session-level lock = crash-safe
        // recovery (R-LEASE-CRASH-SAFE-01). Each active lease holds a
        // dedicated connection from the declared event-store pool until
        // DisposeAsync — callers MUST NOT hold more leases than the pool
        // permits minus headroom for command traffic.
        services.AddSingleton<Whycespace.Shared.Contracts.Infrastructure.Persistence.IDistributedLeaseProvider>(sp =>
            new PostgresAdvisoryLeaseProvider(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<Whycespace.Shared.Kernel.Domain.IClock>()));

        return services;
    }

    /// <summary>
    /// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): builds a single
    /// <see cref="NpgsqlDataSource"/> from a declared
    /// <see cref="PostgresPoolOptions"/>. Pool sizing and timeouts are
    /// applied via <see cref="NpgsqlConnectionStringBuilder"/> so the
    /// caller-supplied connection string remains the source of truth
    /// for host/database/credentials.
    /// </summary>
    internal static NpgsqlDataSource BuildDataSource(PostgresPoolOptions options)
    {
        if (options.MaxPoolSize < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.MaxPoolSize,
                $"PostgresPoolOptions[{options.PoolName}].MaxPoolSize must be at least 1.");
        if (options.MinPoolSize < 0)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.MinPoolSize,
                $"PostgresPoolOptions[{options.PoolName}].MinPoolSize must be 0 or greater.");
        if (options.ConnectionTimeoutSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.ConnectionTimeoutSeconds,
                $"PostgresPoolOptions[{options.PoolName}].ConnectionTimeoutSeconds must be at least 1.");
        if (options.CommandTimeoutSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.CommandTimeoutSeconds,
                $"PostgresPoolOptions[{options.PoolName}].CommandTimeoutSeconds must be at least 1.");

        var csb = new NpgsqlConnectionStringBuilder(options.ConnectionString)
        {
            MaxPoolSize = options.MaxPoolSize,
            MinPoolSize = options.MinPoolSize,
            Timeout = options.ConnectionTimeoutSeconds,
            CommandTimeout = options.CommandTimeoutSeconds,
            Pooling = true,
        };
        return new NpgsqlDataSourceBuilder(csb.ConnectionString).Build();
    }
}
