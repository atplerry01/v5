using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Whyce.Platform.Host.Adapters;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Composition.Infrastructure.Database;

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
        services.AddSingleton(new EventStoreDataSource(BuildDataSource(eventStorePoolOptions)));
        services.AddSingleton(new ChainDataSource(BuildDataSource(chainPoolOptions)));
        services.AddSingleton(new ProjectionsDataSource(BuildDataSource(projectionsPoolOptions)));

        // phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): canonical
        // catalog of declared pool name → MaxPoolSize. Read by
        // PostgresPoolSnapshotProvider so the HC-6 snapshot's
        // MaxConnections field reflects the declared envelope rather
        // than a guess. The catalog must be derived from the same
        // PostgresPoolOptions instances used to build the data
        // sources above so the two cannot drift.
        services.AddSingleton(new Whyce.Platform.Host.Health.PostgresPoolCatalog(
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
