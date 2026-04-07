using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Generic Kafka projection consumer worker (Phase B2b).
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
    private readonly string _kafkaBootstrapServers;
    private readonly string _topic;
    private readonly string _consumerGroup;
    private readonly EventDeserializer _deserializer;
    private readonly ProjectionRegistry _projectionRegistry;
    private readonly IPostgresProjectionWriter _writer;
    private readonly IClock _clock;
    private readonly TimeSpan _pollTimeout;

    public GenericKafkaProjectionConsumerWorker(
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        EventDeserializer deserializer,
        ProjectionRegistry projectionRegistry,
        IPostgresProjectionWriter writer,
        IClock clock,
        TimeSpan? pollTimeout = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _topic = topic;
        _consumerGroup = consumerGroup;
        _deserializer = deserializer;
        _projectionRegistry = projectionRegistry;
        _writer = writer;
        _clock = clock;
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

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(_pollTimeout);
                if (result is null) continue;

                var eventType = ExtractHeader(result.Message.Headers, "event-type");
                var correlationId = ExtractHeader(result.Message.Headers, "correlation-id");
                var eventIdHeader = ExtractHeader(result.Message.Headers, "event-id");
                var aggregateIdHeader = ExtractHeader(result.Message.Headers, "aggregate-id");
                var rawPayload = result.Message.Value;

                if (string.IsNullOrEmpty(eventType))
                {
                    consumer.Commit(result);
                    continue;
                }

                // Kafka guard rule 11 + Phase-2 envelope-truth: projection envelopes MUST
                // reflect the original event's identity. Missing identity headers indicate
                // a malformed message; log + commit + skip (mirrors event-type behavior).
                if (string.IsNullOrEmpty(eventIdHeader) || string.IsNullOrEmpty(aggregateIdHeader))
                {
                    Console.WriteLine($"[KAFKA] Missing event-id/aggregate-id headers on {_topic} for {eventType}; skipping.");
                    consumer.Commit(result);
                    continue;
                }

                if (!Guid.TryParse(eventIdHeader, out var parsedEventId) ||
                    !Guid.TryParse(aggregateIdHeader, out var parsedAggregateId))
                {
                    Console.WriteLine($"[KAFKA] Unparseable event-id/aggregate-id headers on {_topic} for {eventType}; skipping.");
                    consumer.Commit(result);
                    continue;
                }

                Console.WriteLine($"[KAFKA] Consumed: {eventType} from {_topic}");
                var @event = _deserializer.DeserializeInbound(eventType, rawPayload);
                Console.WriteLine($"[KAFKA] Deserialized: {@event?.GetType().Name}");

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
                Console.WriteLine($"[KAFKA] Handlers count: {handlers.Count} for {eventType}");
                foreach (var handler in handlers)
                {
                    Console.WriteLine($"[KAFKA] Executing handler: {handler.GetType().Name}");
                    await handler.HandleAsync(envelope);
                }

                // When a domain handler is registered for this event type, the handler
                // owns the merged state write. Skip the generic raw-payload writer to
                // avoid clobbering the materialized read model.
                if (handlers.Count == 0)
                {
                    Console.WriteLine($"[KAFKA] Writing projection for: {eventType}");
                    await _writer.WriteAsync(eventType, @event, correlationId, stoppingToken);
                }

                consumer.Commit(result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"[KAFKA] ConsumeException on {_topic}: {ex.Error.Reason}");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KAFKA] Exception on {_topic}: {ex.GetType().Name}: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        consumer.Close();
    }

    private static string ExtractHeader(Headers headers, string key)
    {
        var header = headers.FirstOrDefault(h => h.Key == key);
        return header is null ? string.Empty : Encoding.UTF8.GetString(header.GetValueBytes());
    }
}
