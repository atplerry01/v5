using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var postgresEventStoreCs = configuration.GetValue<string>("Postgres__ConnectionString")
            ?? throw new InvalidOperationException("Postgres__ConnectionString is required. No fallback.");
        var postgresChainCs = configuration.GetValue<string>("Postgres__ChainConnectionString")
            ?? postgresEventStoreCs;
        var postgresOutboxCs = postgresEventStoreCs;
        var redisConnectionString = configuration.GetValue<string>("Redis__ConnectionString")
            ?? throw new InvalidOperationException("Redis__ConnectionString is required. No fallback.");
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka__BootstrapServers")
            ?? throw new InvalidOperationException("Kafka__BootstrapServers is required. No fallback.");
        var opaEndpoint = configuration.GetValue<string>("OPA__Endpoint")
            ?? throw new InvalidOperationException("OPA__Endpoint is required. No fallback.");

        // --- Event store + schema-driven deserializer ---
        services.AddSingleton<IEventStore>(sp =>
            new PostgresEventStoreAdapter(
                postgresEventStoreCs,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<IIdGenerator>()));
        services.AddSingleton<EventDeserializer>();

        // --- Chain anchor ---
        services.AddSingleton<IChainAnchor>(sp =>
            new WhyceChainPostgresAdapter(postgresChainCs, sp.GetRequiredService<IClock>()));

        // --- Redis ---
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<IRedisClient>(sp =>
            new StackExchangeRedisClient(sp.GetRequiredService<IConnectionMultiplexer>()));

        // --- Policy evaluator (OPA) ---
        services.AddSingleton<IPolicyEvaluator>(_ =>
            new OpaPolicyEvaluator(new HttpClient { Timeout = TimeSpan.FromSeconds(5) }, opaEndpoint));

        // --- Idempotency, outbox, workflow state ---
        services.AddSingleton<IIdempotencyStore>(_ =>
            new PostgresIdempotencyStoreAdapter(postgresEventStoreCs));
        services.AddSingleton<ISequenceStore>(_ =>
            new PostgresSequenceStoreAdapter(postgresEventStoreCs));
        services.AddSingleton<IOutbox>(_ =>
            new PostgresOutboxAdapter(postgresOutboxCs));

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
        var outboxMaxRetry = configuration.GetValue<int?>("Outbox__MaxRetry")
            ?? new OutboxOptionsRecord().MaxRetry;
        var outboxOptions = new OutboxOptionsRecord { MaxRetry = outboxMaxRetry };
        services.AddSingleton(outboxOptions);

        // --- Kafka Outbox Publisher (background relay: Postgres outbox → Kafka) ---
        services.AddHostedService(sp =>
            new KafkaOutboxPublisher(
                postgresOutboxCs,
                sp.GetRequiredService<IProducer<string, string>>(),
                sp.GetRequiredService<OutboxOptionsRecord>()));

        return services;
    }
}
