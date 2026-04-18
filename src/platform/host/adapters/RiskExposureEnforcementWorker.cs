using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 6 Final Patch — Kafka consumer that bridges
/// <c>ExposureBreachedEvent</c> envelopes on
/// <c>whyce.economic.risk.exposure.events</c> into the enforcement pipeline
/// via <see cref="RiskExposureEnforcementHandler"/>. Closes the last gap
/// in the risk → enforcement loop: without this worker the breach event
/// lands in the outbox and the corresponding Kafka topic but nothing
/// consumes it to drive a <c>DetectViolationCommand</c>.
///
/// Mirrors the <see cref="EnforcementDetectionWorker"/> /
/// <see cref="ReconciliationLifecycleWorker"/> shape:
///   - dedicated consumer group (offset progress independent of the
///     projection pipeline)
///   - manual commit after the handler returns
///   - malformed messages committed-and-skipped so the poll loop never
///     stalls
///   - non-breach envelopes committed-and-ignored (broad topic
///     subscription, narrow event-type filter in the handler).
///
/// The worker subscribes to the whole risk.exposure events topic rather
/// than event-type-filtered because Kafka subscriptions are topic-level.
/// The handler itself filters on <c>EventType == "ExposureBreachedEvent"</c>
/// before dispatching any enforcement command.
/// </summary>
public sealed class RiskExposureEnforcementWorker : BackgroundService
{
    private static readonly string[] Topics =
    [
        "whyce.economic.risk.exposure.events",
    ];

    private const string ConsumerGroup = "whyce.integration.risk-exposure-enforcement";

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly RiskExposureEnforcementHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<RiskExposureEnforcementWorker>? _logger;

    public RiskExposureEnforcementWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        RiskExposureEnforcementHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<RiskExposureEnforcementWorker>? logger = null)
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
            SessionTimeoutMs = _consumerOptions.SessionTimeoutMs,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topics);

        _logger?.LogInformation(
            "RiskExposureEnforcementWorker subscribed to {TopicCount} topics under consumer group {Group}.",
            Topics.Length, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

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

                // Fast-path filter: the handler itself checks the event
                // type, but short-circuiting here avoids needless
                // deserialization for the four non-breach risk events
                // that share the same topic.
                if (eventType != "ExposureBreachedEvent")
                {
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
                    Timestamp = _clock.UtcNow,
                };

                try
                {
                    await _handler.HandleAsync(envelope, stoppingToken);
                    consumer.Commit(result);
                }
                catch (Exception handlerEx)
                {
                    // F3 failure handling: do NOT commit the offset on
                    // handler failure. Kafka will redeliver on next poll
                    // (or after a consumer restart). This gives automatic
                    // retry for transient failures (dispatch policy
                    // delay, downstream projection lag) without dropping
                    // the breach signal. Outer catch logs and backs off.
                    _logger?.LogError(handlerEx,
                        "RiskExposureEnforcementHandler failed for breach on aggregate {AggregateId}; offset NOT committed, will redeliver.",
                        envelope.AggregateId);
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "RiskExposureEnforcementWorker iteration failed; will continue.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
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
