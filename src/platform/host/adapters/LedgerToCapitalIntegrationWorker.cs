using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// BackgroundService that subscribes to ledger event topics and routes each
/// envelope through <see cref="LedgerToCapitalIntegrationHandler"/>. Mirrors
/// <see cref="WorkflowTriggerWorker"/> in shape, but runs on its own consumer
/// group so commit progress is independent of the projection pipeline.
///
/// Subscribed topics:
///   - whyce.economic.ledger.journal.events  (source of JournalEntryRecordedEvent)
///   - whyce.economic.ledger.ledger.events   (source of LedgerUpdatedEvent, observation-only)
///
/// Transaction boundary: events on these topics are produced by the outbox
/// relay after the ledger aggregate's write transaction commits, so the
/// worker only ever acts on already-committed flows.
///
/// Failure handling: malformed headers/payloads are skipped with a warning
/// and the offset IS committed — the projection worker for the same topic
/// already DLQs malformed messages. Handler exceptions leave the offset
/// un-committed, so the message is re-delivered after restart; replay safety
/// is owned by the handler's envelope-level idempotency claim.
/// </summary>
public sealed class LedgerToCapitalIntegrationWorker : BackgroundService
{
    private static readonly string[] Topics =
    [
        "whyce.economic.ledger.journal.events",
        "whyce.economic.ledger.ledger.events"
    ];

    private const string ConsumerGroup = "whyce.integration.ledger-to-capital";

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly LedgerToCapitalIntegrationHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<LedgerToCapitalIntegrationWorker>? _logger;

    public LedgerToCapitalIntegrationWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        LedgerToCapitalIntegrationHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<LedgerToCapitalIntegrationWorker>? logger = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _deserializer = deserializer;
        _handler = handler;
        _clock = clock;
        _consumerOptions = consumerOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            QueuedMaxMessagesKbytes = _consumerOptions.QueuedMaxMessagesKbytes,
            MessageMaxBytes = _consumerOptions.FetchMessageMaxBytes,
            MaxPollIntervalMs = _consumerOptions.MaxPollIntervalMs,
            SessionTimeoutMs = _consumerOptions.SessionTimeoutMs
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topics);

        _logger?.LogInformation(
            "LedgerToCapitalIntegrationWorker subscribed to {TopicCount} topics under consumer group {Group}.",
            Topics.Length, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                    continue;

                var eventType = ExtractHeader(result.Message.Headers, "event-type");
                var eventIdHeader = ExtractHeader(result.Message.Headers, "event-id");
                var aggregateIdHeader = ExtractHeader(result.Message.Headers, "aggregate-id");
                var correlationHeader = ExtractHeader(result.Message.Headers, "correlation-id");
                var causationHeader = ExtractHeader(result.Message.Headers, "causation-id");

                if (string.IsNullOrEmpty(eventType)
                    || string.IsNullOrEmpty(eventIdHeader)
                    || string.IsNullOrEmpty(aggregateIdHeader)
                    || !Guid.TryParse(eventIdHeader, out var parsedEventId)
                    || !Guid.TryParse(aggregateIdHeader, out var parsedAggregateId))
                {
                    _logger?.LogWarning(
                        "Skipping malformed message on {Topic}: missing or unparseable headers.",
                        result.Topic);
                    consumer.Commit(result);
                    continue;
                }

                var payload = _deserializer.DeserializeInbound(eventType, result.Message.Value);

                var envelope = new EventEnvelope
                {
                    EventId = parsedEventId,
                    AggregateId = parsedAggregateId,
                    CorrelationId = Guid.TryParse(correlationHeader, out var cid) ? cid : Guid.Empty,
                    CausationId = Guid.TryParse(causationHeader, out var causId) ? causId : Guid.Empty,
                    EventType = eventType,
                    EventName = eventType,
                    EventVersion = EventVersion.Default,
                    SchemaHash = string.Empty,
                    Payload = payload,
                    ExecutionHash = string.Empty,
                    PolicyHash = string.Empty,
                    Timestamp = _clock.UtcNow
                };

                await _handler.HandleAsync(envelope, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "LedgerToCapitalIntegrationWorker iteration failed; will continue.");
                // Offset is NOT committed — message will be re-delivered after restart.
            }
        }

        consumer.Close();
    }

    private static string? ExtractHeader(Headers? headers, string key)
    {
        if (headers is null) return null;
        foreach (var h in headers)
        {
            if (string.Equals(h.Key, key, StringComparison.Ordinal))
            {
                var bytes = h.GetValueBytes();
                return bytes is null ? null : System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
        return null;
    }
}
