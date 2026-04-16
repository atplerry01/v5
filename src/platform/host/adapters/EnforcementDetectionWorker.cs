using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// BackgroundService that subscribes to the configured set of source topics
/// and routes each envelope through <see cref="EnforcementDetectionHandler"/>.
/// The handler delegates rule matching to WHYCEPOLICY and emits zero-or-more
/// DetectViolationCommand dispatches per source event.
///
/// Default subscribed topics (composable via configuration
/// <c>Enforcement:Detection:Topics</c>):
///   • whyce.economic.ledger.journal.events
///   • whyce.economic.ledger.ledger.events
///
/// Additional topic groups (identity, policy) can be appended to the
/// configuration once those classifications publish to Kafka under canonical
/// names.
/// </summary>
public sealed class EnforcementDetectionWorker : BackgroundService
{
    private const string ConsumerGroup = "whyce.integration.enforcement-detection";

    private readonly string _kafkaBootstrapServers;
    private readonly IReadOnlyList<string> _topics;
    private readonly EventDeserializer _deserializer;
    private readonly EnforcementDetectionHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<EnforcementDetectionWorker>? _logger;

    public EnforcementDetectionWorker(
        string kafkaBootstrapServers,
        IReadOnlyList<string> topics,
        EventDeserializer deserializer,
        EnforcementDetectionHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<EnforcementDetectionWorker>? logger = null)
    {
        if (topics is null || topics.Count == 0)
            throw new ArgumentException("At least one source topic is required.", nameof(topics));

        _kafkaBootstrapServers = kafkaBootstrapServers;
        _topics = topics;
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
        consumer.Subscribe(_topics);

        _logger?.LogInformation(
            "EnforcementDetectionWorker subscribed to {TopicCount} topics under consumer group {Group}.",
            _topics.Count, ConsumerGroup);

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

                object payload;
                try
                {
                    payload = _deserializer.DeserializeInbound(eventType, result.Message.Value);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex,
                        "Unable to deserialize event {EventType} on {Topic}; skipping.",
                        eventType, result.Topic);
                    consumer.Commit(result);
                    continue;
                }

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
                _logger?.LogError(ex, "EnforcementDetectionWorker iteration failed; will continue.");
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
