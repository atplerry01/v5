using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
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
    private const string WorkerName = "risk-exposure-enforcement"; // R2.E.1 rebalance-metric tag

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly RiskExposureEnforcementHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<RiskExposureEnforcementWorker>? _logger;

    // R2.A.3d Phase B: retry-tier escalation wiring.
    private readonly IProducer<string, string>? _producer;
    private readonly TopicNameResolver? _topicNameResolver;
    private readonly RetryTierOptions? _retryOptions;
    private readonly IRandomProvider? _randomProvider;
    private readonly ICircuitBreaker? _kafkaBreaker;

    public RiskExposureEnforcementWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        RiskExposureEnforcementHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<RiskExposureEnforcementWorker>? logger = null,
        IProducer<string, string>? producer = null,
        TopicNameResolver? topicNameResolver = null,
        RetryTierOptions? retryOptions = null,
        IRandomProvider? randomProvider = null,
        ICircuitBreaker? kafkaBreaker = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _deserializer = deserializer;
        _handler = handler;
        _clock = clock;
        _consumerOptions = consumerOptions;
        _logger = logger;
        _producer = producer;
        _topicNameResolver = topicNameResolver;
        _retryOptions = retryOptions;
        _randomProvider = randomProvider;
        _kafkaBreaker = kafkaBreaker;
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
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky, // R2.E.1
        };

        var consumerBuilder = new ConsumerBuilder<string, string>(config);
        KafkaRebalanceObservability.Attach(consumerBuilder, Topics[0], WorkerName, _logger);
        using var consumer = consumerBuilder.Build();
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

                KafkaLagObservability.Record(consumer, result, WorkerName, Topics[0]);

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
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception handlerEx)
                {
                    // R2.A.3d Phase B / R-RETRY-CONSUMER-INTEGRATION-02:
                    // route through the retry escalator when wired.
                    // F3 fallback: when retry tier is not wired, do NOT
                    // commit; Kafka redelivers next poll.
                    await EscalateOrRedeliverAsync(
                        consumer, result, parsedEventId, handlerEx, stoppingToken);
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

    private async Task EscalateOrRedeliverAsync(
        IConsumer<string, string> consumer,
        ConsumeResult<string, string> result,
        Guid parsedEventId,
        Exception handlerEx,
        CancellationToken ct)
    {
        if (_producer is null || _topicNameResolver is null || _retryOptions is null
            || _randomProvider is null || _kafkaBreaker is null)
        {
            _logger?.LogError(handlerEx,
                "RiskExposureEnforcementWorker handler failed on {Topic} p{Partition} o{Offset}; retry tier not wired, will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value);
            return;
        }

        var priorAttempt = RetryHeaders.ReadPriorAttemptCount(result.Message.Headers);
        try
        {
            await KafkaRetryEscalator.EscalateAsync(
                _producer, _topicNameResolver, result.Topic, result.Message,
                parsedEventId, $"{handlerEx.GetType().Name}: {handlerEx.Message}",
                priorAttempt, _retryOptions.MaxAttempts,
                _retryOptions.BaseBackoff, _retryOptions.MaxBackoff,
                _randomProvider, _clock, _kafkaBreaker, _logger, ct);
            consumer.Commit(result);
        }
        catch (CircuitBreakerOpenException breakerEx)
        {
            _logger?.LogWarning(
                "RiskExposureEnforcementWorker handler failed on {Topic} p{Partition} o{Offset}; retry-escalate blocked by breaker '{Breaker}' ({RetryAfter}s); will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value,
                breakerEx.BreakerName, breakerEx.RetryAfterSeconds);
        }
        catch (Exception escalationEx)
        {
            _logger?.LogError(escalationEx,
                "RiskExposureEnforcementWorker retry escalation failed on {Topic} p{Partition} o{Offset} after handler exception {HandlerException}; will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value, handlerEx.GetType().Name);
        }
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
