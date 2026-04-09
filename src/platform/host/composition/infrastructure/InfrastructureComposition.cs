using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StackExchange.Redis;
using Whyce.Platform.Host.Adapters;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using OutboxOptionsRecord = Whyce.Shared.Contracts.Infrastructure.Messaging.OutboxOptions;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Composition.Infrastructure;

/// <summary>
/// Infrastructure adapters: Postgres (event store, chain, idempotency, outbox),
/// Redis, OPA, Kafka producer, and the Kafka outbox publisher hosted relay.
/// All connection strings are read from configuration with no hardcoded fallback,
/// matching the previous Program.cs behavior exactly.
/// </summary>
public static class InfrastructureComposition
{
    public static IServiceCollection AddInfrastructureComposition(
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
        // TodoBootstrap) and Projections:ConnectionString (read by
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

        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString")
            ?? throw new InvalidOperationException("Redis:ConnectionString is required. No fallback.");
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");
        var opaEndpoint = configuration.GetValue<string>("OPA:Endpoint")
            ?? throw new InvalidOperationException("OPA:Endpoint is required. No fallback.");

        // --- Event store + schema-driven deserializer ---
        services.AddSingleton<IEventStore>(sp =>
            new PostgresEventStoreAdapter(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<IIdGenerator>()));
        services.AddSingleton<EventDeserializer>();

        // --- Chain anchor ---
        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // register the concrete WhyceChainPostgresAdapter singleton
        // first, then forward IChainAnchor to it. RuntimeStateAggregator
        // depends on the concrete type so it can read the new
        // side-effect-free IsBreakerOpen getter — adding the getter
        // to IChainAnchor would widen scope into the TC-3 contract.
        services.AddSingleton<WhyceChainPostgresAdapter>(sp =>
            new WhyceChainPostgresAdapter(
                sp.GetRequiredService<ChainDataSource>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Admission.ChainAnchorOptions>()));
        services.AddSingleton<IChainAnchor>(sp => sp.GetRequiredService<WhyceChainPostgresAdapter>());

        // --- Redis ---
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IRedisClient>(sp =>
            new StackExchangeRedisClient(sp.GetRequiredService<IConnectionMultiplexer>()));

        // --- Policy evaluator (OPA) ---
        // phase1.5-S5.2.1 / PC-2 (OPA-CONFIG-01): bind OpaOptions from
        // configuration following the phase1.6-S1.5 OutboxOptions
        // precedent — a plain record constructed at the composition root,
        // no IOptions<T> indirection. The HttpClient.Timeout is left at
        // the .NET default; the per-call envelope is enforced by the
        // evaluator's linked CTS sized from OpaOptions.RequestTimeoutMs.
        var opaOptions = new OpaOptions
        {
            Endpoint = opaEndpoint,
            RequestTimeoutMs = configuration.GetValue<int?>("Opa:RequestTimeoutMs")
                ?? new OpaOptions().RequestTimeoutMs,
            BreakerThreshold = configuration.GetValue<int?>("Opa:BreakerThreshold")
                ?? new OpaOptions().BreakerThreshold,
            BreakerWindowSeconds = configuration.GetValue<int?>("Opa:BreakerWindowSeconds")
                ?? new OpaOptions().BreakerWindowSeconds,
            OpenStateBehavior = configuration.GetValue<string>("Opa:OpenStateBehavior")
                ?? new OpaOptions().OpenStateBehavior,
        };
        services.AddSingleton(opaOptions);
        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // mirror of the chain-anchor pattern above. Register the
        // concrete OpaPolicyEvaluator first so RuntimeStateAggregator
        // can read its side-effect-free IsBreakerOpen getter; then
        // forward IPolicyEvaluator to the same singleton.
        services.AddSingleton<OpaPolicyEvaluator>(sp =>
            new OpaPolicyEvaluator(
                new HttpClient(),
                sp.GetRequiredService<OpaOptions>(),
                sp.GetRequiredService<IClock>()));
        services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<OpaPolicyEvaluator>());

        // --- Idempotency, outbox, workflow state ---
        services.AddSingleton<IIdempotencyStore>(sp =>
            new PostgresIdempotencyStoreAdapter(sp.GetRequiredService<EventStoreDataSource>()));
        services.AddSingleton<ISequenceStore>(sp =>
            new PostgresSequenceStoreAdapter(sp.GetRequiredService<EventStoreDataSource>()));
        // phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): the outbox depth
        // snapshot is a singleton seam written by OutboxDepthSampler
        // and read by PostgresOutboxAdapter. Both are registered below.
        services.AddSingleton<IOutboxDepthSnapshot, OutboxDepthSnapshot>();
        services.AddSingleton<IOutbox>(sp =>
            new PostgresOutboxAdapter(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<IOutboxDepthSnapshot>(),
                sp.GetRequiredService<OutboxOptionsRecord>(),
                sp.GetRequiredService<IClock>()));

        // --- Kafka producer (for outbox relay) ---
        // phase1-gate-H7-H9-safe:
        //   • EnableIdempotence: dedup on broker, implies Acks=All + MaxInFlight≤5,
        //     prevents duplicate writes on transient producer retries.
        //   • CompressionType.Lz4: batch-level compression for JSON payloads,
        //     consumer auto-decompresses, no client changes required.
        services.AddSingleton<IProducer<string, string>>(_ =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaBootstrapServers,
                EnableIdempotence = true,
                CompressionType = CompressionType.Lz4,
            };
            return new ProducerBuilder<string, string>(config).Build();
        });

        // --- Outbox tunables (phase1.6-S1.5 / OUTBOX-CONFIG-01) ---
        // Read from configuration with the OutboxOptions default as the
        // fallback. The default matches the pre-S1.5 hardcoded value (5),
        // so unconfigured deployments are byte-identical to the previous
        // behavior. Validation lives in the OutboxOptions/publisher
        // constructor — composition only resolves the value.
        // phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): bind the new
        // depth/sampling/saturation tunables alongside the existing
        // MaxRetry. Defaults from the OutboxOptions record itself, so
        // an unconfigured deployment is safer (real watermark + real
        // sampler) than the pre-S5.2.1 unbounded behavior.
        var outboxDefaults = new OutboxOptionsRecord();
        var outboxOptions = new OutboxOptionsRecord
        {
            MaxRetry = configuration.GetValue<int?>("Outbox:MaxRetry") ?? outboxDefaults.MaxRetry,
            HighWaterMark = configuration.GetValue<int?>("Outbox:HighWaterMark")
                ?? outboxDefaults.HighWaterMark,
            SamplingIntervalSeconds = configuration.GetValue<int?>("Outbox:SamplingIntervalSeconds")
                ?? outboxDefaults.SamplingIntervalSeconds,
            SaturationResponse = configuration.GetValue<string>("Outbox:SaturationResponse")
                ?? outboxDefaults.SaturationResponse,
            RetryAfterSeconds = configuration.GetValue<int?>("Outbox:RetryAfterSeconds")
                ?? outboxDefaults.RetryAfterSeconds,
            // phase1.5-S5.2.2 / KC-3 (DLQ-OBSERVABILITY-01): declared
            // deadletter handling policy. Default "operator-managed";
            // override via Outbox:DeadletterRetention to declare
            // intent for a future auto-prune workstream.
            DeadletterRetention = configuration.GetValue<string>("Outbox:DeadletterRetention")
                ?? outboxDefaults.DeadletterRetention,
            // phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01):
            // declared snapshot freshness ceiling. Defaults to 10s
            // (2 × SamplingIntervalSeconds). Closes H19.
            SnapshotMaxAgeSeconds = configuration.GetValue<int?>("Outbox:SnapshotMaxAgeSeconds")
                ?? outboxDefaults.SnapshotMaxAgeSeconds,
        };
        services.AddSingleton(outboxOptions);

        // --- Kafka consumer tunables (phase1.5-S5.2.1 / PC-6 KAFKA-CONSUMER-CONFIG-01) ---
        // Bind the four declared buffering / session / poll parameters
        // from configuration with KafkaConsumerOptions defaults as the
        // fallback. Every GenericKafkaProjectionConsumerWorker instance
        // resolves this singleton via DI and applies it to its
        // ConsumerConfig — no worker constructs an unbounded prefetch
        // shape any more.
        var kafkaConsumerDefaults = new KafkaConsumerOptions();
        var kafkaConsumerOptions = new KafkaConsumerOptions
        {
            QueuedMaxMessagesKbytes = configuration.GetValue<int?>("KafkaConsumer:QueuedMaxMessagesKbytes")
                ?? kafkaConsumerDefaults.QueuedMaxMessagesKbytes,
            FetchMessageMaxBytes = configuration.GetValue<int?>("KafkaConsumer:FetchMessageMaxBytes")
                ?? kafkaConsumerDefaults.FetchMessageMaxBytes,
            MaxPollIntervalMs = configuration.GetValue<int?>("KafkaConsumer:MaxPollIntervalMs")
                ?? kafkaConsumerDefaults.MaxPollIntervalMs,
            SessionTimeoutMs = configuration.GetValue<int?>("KafkaConsumer:SessionTimeoutMs")
                ?? kafkaConsumerDefaults.SessionTimeoutMs,
        };
        services.AddSingleton(kafkaConsumerOptions);

        // phase1.5-S5.2.1 / PC-3: outbox depth sampler — periodic probe
        // that publishes the latest count(*) and oldest-pending-age to
        // the shared snapshot and exports both as gauges on the
        // Whyce.Outbox meter.
        services.AddHostedService(sp =>
            new OutboxDepthSampler(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<IOutboxDepthSnapshot>(),
                sp.GetRequiredService<OutboxOptionsRecord>(),
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetRequiredService<IClock>()));

        // --- Kafka Outbox Publisher (background relay: Postgres outbox → Kafka) ---
        // phase1.6-S1.6 (DLQ-RESOLVER-01): TopicNameResolver is registered
        // as a singleton by ProjectionComposition; resolve it from DI here
        // and thread it through. The publisher uses it for dead-letter
        // topic resolution — there is no inline string manipulation left.
        services.AddHostedService(sp =>
            new KafkaOutboxPublisher(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<IProducer<string, string>>(),
                sp.GetRequiredService<TopicNameResolver>(),
                sp.GetRequiredService<OutboxOptionsRecord>(),
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetRequiredService<IClock>()));

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
    private static NpgsqlDataSource BuildDataSource(PostgresPoolOptions options)
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
