using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using OutboxOptionsRecord = Whycespace.Shared.Contracts.Infrastructure.Messaging.OutboxOptions;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Messaging;

/// <summary>
/// Messaging capability — Kafka producer, consumer options, outbox adapter,
/// outbox tunables, outbox depth sampler, and outbox publisher relay.
/// </summary>
public static class KafkaInfrastructureModule
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        // R2.E.4 / R-TOPIC-ALIGNMENT-01: startup-time topic alignment
        // verifier. Registered FIRST so it runs before projection /
        // saga / integration workers attach to the broker — a missing
        // topic fails fast at startup (when FailIfTopicsMissing=true) or
        // logs WARN (default) instead of degrading to mysterious
        // UNKNOWN_TOPIC_OR_PARTITION errors at consume time. Topic
        // CREATION remains the responsibility of
        // infrastructure/event-fabric/kafka/create-topics.sh — this
        // service is verification-only.
        var verifyTopicsOnStartup = configuration.GetValue<bool?>("Kafka:VerifyTopicsOnStartup") ?? true;
        var failIfTopicsMissing = configuration.GetValue<bool?>("Kafka:FailIfTopicsMissing") ?? false;
        var topicMetadataTimeoutMs = configuration.GetValue<int?>("Kafka:TopicMetadataTimeoutMs") ?? 10_000;
        services.AddHostedService(sp => new TopicProvisioningHostedService(
            kafkaBootstrapServers,
            verifyTopicsOnStartup,
            failIfTopicsMissing,
            TimeSpan.FromMilliseconds(topicMetadataTimeoutMs),
            sp.GetService<Microsoft.Extensions.Logging.ILogger<TopicProvisioningHostedService>>()));

        // --- Outbox adapter (Postgres-backed, Kafka-relayed) ---
        // phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): the outbox depth
        // snapshot is a singleton seam written by OutboxDepthSampler
        // and read by PostgresOutboxAdapter. Both are registered below.
        services.AddSingleton<IOutboxDepthSnapshot, OutboxDepthSnapshot>();
        services.AddSingleton<IOutbox>(sp =>
            new PostgresOutboxAdapter(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<IOutboxDepthSnapshot>(),
                sp.GetRequiredService<OutboxOptionsRecord>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<EventSchemaRegistry>()));

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

        // R2.A.D.3b / R-KAFKA-BREAKER-01: shared "kafka-producer" breaker.
        // Protects all three ProduceAsync call sites (outbox primary publish,
        // outbox DLQ publish, consumer DLQ publish). Registered as a plain
        // ICircuitBreaker contributor so CircuitBreakerRegistry picks it up
        // alongside OPA + Chain. Defaults: 5 failures / 30s window — tune
        // via Kafka:BreakerThreshold and Kafka:BreakerWindowSeconds.
        var kafkaBreakerThreshold = configuration.GetValue<int?>("Kafka:BreakerThreshold") ?? 5;
        var kafkaBreakerWindowSeconds = configuration.GetValue<int?>("Kafka:BreakerWindowSeconds") ?? 30;
        services.AddSingleton<ICircuitBreaker>(sp =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions
                {
                    Name = "kafka-producer",
                    FailureThreshold = kafkaBreakerThreshold,
                    WindowSeconds = kafkaBreakerWindowSeconds
                },
                sp.GetRequiredService<IClock>()));

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
        // Whycespace.Outbox meter.
        services.AddHostedService(sp =>
            new OutboxDepthSampler(
                sp.GetRequiredService<EventStoreDataSource>(),
                sp.GetRequiredService<IOutboxDepthSnapshot>(),
                sp.GetRequiredService<OutboxOptionsRecord>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetRequiredService<IClock>()));

        // --- R2.A.3d Retry Tier Infrastructure ---
        // R-RETRY-CONSUMER-WORKER-01: single consumer instance subscribed
        // to every `.retry` topic declared in KafkaCanonicalTopics.All.
        // Dedicated consumer group isolates retry-tier offset progress
        // from projection / saga / integration consumer groups.
        //
        // Configuration:
        //   Kafka:Retry:MaxAttempts             — default 5
        //   Kafka:Retry:BaseBackoffSeconds      — default 5 (first retry waits ~5s)
        //   Kafka:Retry:MaxBackoffSeconds       — default 300 (5 min cap)
        var retryMaxAttempts = configuration.GetValue<int?>("Kafka:Retry:MaxAttempts") ?? 5;
        var retryBaseBackoffSeconds = configuration.GetValue<int?>("Kafka:Retry:BaseBackoffSeconds") ?? 5;
        var retryMaxBackoffSeconds = configuration.GetValue<int?>("Kafka:Retry:MaxBackoffSeconds") ?? 300;
        if (retryMaxAttempts < 1)
            throw new InvalidOperationException("Kafka:Retry:MaxAttempts must be at least 1.");
        if (retryBaseBackoffSeconds < 1)
            throw new InvalidOperationException("Kafka:Retry:BaseBackoffSeconds must be at least 1.");
        if (retryMaxBackoffSeconds < retryBaseBackoffSeconds)
            throw new InvalidOperationException("Kafka:Retry:MaxBackoffSeconds must be >= BaseBackoffSeconds.");

        services.AddSingleton(new RetryTierOptions(
            MaxAttempts: retryMaxAttempts,
            BaseBackoff: TimeSpan.FromSeconds(retryBaseBackoffSeconds),
            MaxBackoff: TimeSpan.FromSeconds(retryMaxBackoffSeconds)));

        services.AddHostedService(sp =>
        {
            var retryTopics = KafkaCanonicalTopics.All
                .Where(t => t.EndsWith(".retry", StringComparison.Ordinal))
                .ToList();
            return new KafkaRetryConsumerWorker(
                kafkaBootstrapServers,
                retryTopics,
                sp.GetRequiredService<IProducer<string, string>>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<KafkaRetryConsumerWorker>>());
        });

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
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetRequiredService<IClock>(),
                pollInterval: null,
                logger: sp.GetService<Microsoft.Extensions.Logging.ILogger<KafkaOutboxPublisher>>(),
                // R2.A.3b: mirror deadletter Kafka publishes to durable store.
                deadLetterStore: sp.GetService<Whycespace.Shared.Contracts.Infrastructure.Messaging.IDeadLetterStore>(),
                // R2.A.D.3b / R-KAFKA-BREAKER-01: wrap ProduceAsync in the
                // shared "kafka-producer" breaker. Resolved via registry
                // (which picks up the ICircuitBreaker registered above).
                kafkaBreaker: sp.GetRequiredService<ICircuitBreakerRegistry>().Get("kafka-producer")));

        return services;
    }
}
