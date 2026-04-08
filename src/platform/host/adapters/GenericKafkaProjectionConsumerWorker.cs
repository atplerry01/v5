using System.Diagnostics.Metrics;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Generic Kafka projection consumer worker.
/// Replaces the per-domain KafkaProjectionConsumerWorker — this file contains ZERO domain references.
///
/// Per-domain configuration (topic, consumer group, projection table) is supplied by the
/// domain bootstrap module via constructor injection. One worker instance is registered
/// per (topic, projection-table) pair.
///
/// Flow: Kafka topic → header extraction → EventDeserializer (schema-driven) →
///       ProjectionRegistry handlers (envelope-based) → IPostgresProjectionWriter
/// </summary>
public sealed class GenericKafkaProjectionConsumerWorker : BackgroundService
{
    // phase1-gate-S6: observability meter shared across all worker instances.
    public static readonly Meter Meter = new("Whyce.Projection.Consumer", "1.0");
    private static readonly Counter<long> ConsumedCounter = Meter.CreateCounter<long>("consumer.consumed");
    private static readonly Counter<long> DlqRoutedCounter = Meter.CreateCounter<long>("consumer.dlq_routed");
    private static readonly Counter<long> HandlerInvokedCounter = Meter.CreateCounter<long>("consumer.handler_invoked");

    private readonly string _kafkaBootstrapServers;
    private readonly string _topic;
    private readonly string _consumerGroup;
    private readonly EventDeserializer _deserializer;
    private readonly ProjectionRegistry _projectionRegistry;
    private readonly IPostgresProjectionWriter _writer;
    private readonly IClock _clock;
    private readonly ILogger<GenericKafkaProjectionConsumerWorker>? _logger;
    private readonly TimeSpan _pollTimeout;

    public GenericKafkaProjectionConsumerWorker(
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        EventDeserializer deserializer,
        ProjectionRegistry projectionRegistry,
        IPostgresProjectionWriter writer,
        IClock clock,
        ILogger<GenericKafkaProjectionConsumerWorker>? logger = null,
        TimeSpan? pollTimeout = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _topic = topic;
        _consumerGroup = consumerGroup;
        _deserializer = deserializer;
        _projectionRegistry = projectionRegistry;
        _writer = writer;
        _clock = clock;
        _logger = logger;
        _pollTimeout = pollTimeout ?? TimeSpan.FromSeconds(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = _consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        // phase1-gate-S3: deadletter topic derived from the source topic by
        // replacing the trailing `.events` segment with `.deadletter`. Topics
        // are pre-provisioned by infrastructure/event-fabric/kafka/create-topics.sh.
        var deadletterTopic = _topic.EndsWith(".events")
            ? string.Concat(_topic.AsSpan(0, _topic.Length - ".events".Length), ".deadletter")
            : _topic + ".deadletter";

        var producerConfig = new ProducerConfig { BootstrapServers = _kafkaBootstrapServers };
        using var deadletterProducer = new ProducerBuilder<string, string>(producerConfig).Build();

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(_pollTimeout);
                if (result is null) continue;

                // phase1.6-S2.3: distinguish missing vs explicitly-empty headers.
                // ExtractHeader now returns null when the key is absent and an
                // empty string when the key is present with an empty value.
                // The DLQ reason strings carry the distinction so operators can
                // tell a producer-side schema bug ("present but blank") apart
                // from a producer-side wiring bug ("never set the header at all").
                var eventType = ExtractHeader(result.Message.Headers, "event-type");
                var correlationId = ExtractHeader(result.Message.Headers, "correlation-id");
                var eventIdHeader = ExtractHeader(result.Message.Headers, "event-id");
                var aggregateIdHeader = ExtractHeader(result.Message.Headers, "aggregate-id");
                var rawPayload = result.Message.Value;

                // phase1-gate-S3: header enforcement. Malformed messages are
                // routed to the deadletter topic — never silently committed and
                // never allowed to crash the consumer.
                if (eventType is null)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        "absent event-type header", stoppingToken);
                    consumer.Commit(result);
                    continue;
                }
                if (eventType.Length == 0)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        "empty event-type header (present but blank)", stoppingToken);
                    consumer.Commit(result);
                    continue;
                }

                if (eventIdHeader is null || aggregateIdHeader is null)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        $"absent event-id/aggregate-id headers (event-type={eventType})",
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }
                if (eventIdHeader.Length == 0 || aggregateIdHeader.Length == 0)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        $"empty event-id/aggregate-id headers (present but blank, event-type={eventType})",
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }

                if (!Guid.TryParse(eventIdHeader, out var parsedEventId) ||
                    !Guid.TryParse(aggregateIdHeader, out var parsedAggregateId))
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        $"unparseable event-id/aggregate-id headers (event-type={eventType})",
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }

                ConsumedCounter.Add(1, new KeyValuePair<string, object?>("topic", _topic));
                _logger?.LogDebug("Consumed {EventType} from {Topic}", eventType, _topic);
                var @event = _deserializer.DeserializeInbound(eventType, rawPayload);

                var envelope = new EventEnvelope
                {
                    EventId = parsedEventId,
                    AggregateId = parsedAggregateId,
                    CorrelationId = Guid.TryParse(correlationId, out var cid) ? cid : Guid.Empty,
                    EventType = eventType,
                    EventName = eventType,
                    EventVersion = EventVersion.Default,
                    SchemaHash = string.Empty,
                    Payload = @event,
                    ExecutionHash = string.Empty,
                    PolicyHash = string.Empty,
                    Timestamp = _clock.UtcNow
                };

                var handlers = _projectionRegistry.ResolveHandlers(eventType).ToList();
                foreach (var handler in handlers)
                {
                    HandlerInvokedCounter.Add(1,
                        new KeyValuePair<string, object?>("event_type", eventType),
                        new KeyValuePair<string, object?>("handler", handler.GetType().Name));
                    await handler.HandleAsync(envelope);
                }

                // When a domain handler is registered for this event type, the handler
                // owns the merged state write. Skip the generic raw-payload writer to
                // avoid clobbering the materialized read model.
                if (handlers.Count == 0)
                {
                    // phase1.6-S2.3: writer expects a non-null correlation id
                    // string. Absent correlation header is non-fatal here —
                    // the writer parses Guid.Empty out of an empty string.
                    await _writer.WriteAsync(eventType, @event, correlationId ?? string.Empty, stoppingToken);
                }

                consumer.Commit(result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger?.LogError(ex, "ConsumeException on {Topic}: {Reason}", _topic, ex.Error.Reason);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error on {Topic}", _topic);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        consumer.Close();
    }

    /// <summary>
    /// phase1.6-S2.3: returns null when the header key is absent (so the
    /// caller can DLQ with reason "absent"), an empty string when the key
    /// is present with a zero-byte value (DLQ reason "present but blank"),
    /// or the UTF-8 decoded value otherwise. Do NOT normalize null → empty
    /// here — the operator distinction between "producer never set this
    /// header" and "producer set it to an empty value" is the entire reason
    /// this method is structured this way.
    /// </summary>
    private static string? ExtractHeader(Headers headers, string key)
    {
        var header = headers.FirstOrDefault(h => h.Key == key);
        if (header is null) return null;
        return Encoding.UTF8.GetString(header.GetValueBytes());
    }

    /// <summary>
    /// phase1-gate-S3: publishes a malformed inbound message to the deadletter
    /// topic preserving original key/value/headers and adding diagnostic
    /// `dlq-reason` and `dlq-source-topic` headers. Failures here are caught
    /// and logged — the consumer never crashes on a deadletter publish error.
    /// </summary>
    private async Task PublishToDeadletterAsync(
        IProducer<string, string> producer,
        string deadletterTopic,
        Message<string, string> original,
        string reason,
        CancellationToken ct)
    {
        try
        {
            var headers = new Headers();
            if (original.Headers is not null)
            {
                foreach (var h in original.Headers)
                    headers.Add(h.Key, h.GetValueBytes());
            }
            headers.Add("dlq-reason",       Encoding.UTF8.GetBytes(reason));
            headers.Add("dlq-source-topic", Encoding.UTF8.GetBytes(_topic));

            // phase1-gate-H7a: enforce per-aggregate Kafka ordering. Prefer the
            // aggregate-id header (set by KafkaOutboxPublisher); fall back to the
            // original key if the header is missing or unparseable so that
            // malformed-header DLQ paths are still routable.
            // phase1.6-S2.3: ExtractHeader now returns null for absent
            // headers (vs empty for present-but-blank). Treat both as
            // "not usable as a partition key" and fall through to
            // original.Key so the DLQ row still routes to a partition.
            var aggregateIdHeader = ExtractHeader(original.Headers ?? new Headers(), "aggregate-id");
            var dlqKey = !string.IsNullOrEmpty(aggregateIdHeader) ? aggregateIdHeader : original.Key;
            var dlqMessage = new Message<string, string>
            {
                Key = dlqKey,
                Value = original.Value,
                Headers = headers
            };

            await producer.ProduceAsync(deadletterTopic, dlqMessage, ct);
            DlqRoutedCounter.Add(1,
                new KeyValuePair<string, object?>("source_topic", _topic),
                new KeyValuePair<string, object?>("reason", reason));
            _logger?.LogWarning(
                "Routed message from {SourceTopic} to {DeadletterTopic}: {Reason}",
                _topic, deadletterTopic, reason);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "FAILED to publish to deadletter topic {DeadletterTopic}", deadletterTopic);
        }
    }
}
