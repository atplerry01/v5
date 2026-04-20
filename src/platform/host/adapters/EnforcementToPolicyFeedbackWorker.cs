using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// BackgroundService that subscribes to the enforcement violation event topic
/// and routes each envelope through
/// <see cref="EnforcementToPolicyFeedbackHandler"/>. Mirrors
/// <see cref="LedgerToCapitalIntegrationWorker"/> in shape so malformed-message
/// handling and offset semantics are identical to the rest of the
/// integration workers.
///
/// Subscribed topic: whyce.economic.enforcement.violation.events
/// Consumer group:   whyce.integration.enforcement-to-policy-feedback
/// </summary>
public sealed class EnforcementToPolicyFeedbackWorker : BackgroundService
{
    private const string SourceTopic = "whyce.economic.enforcement.violation.events";
    private const string ConsumerGroup = "whyce.integration.enforcement-to-policy-feedback";
    // R2.E.1 / R-CONSUMER-REBALANCE-01: canonical worker name for
    // rebalance-metric tags + future HC-5 liveness cross-reference.
    private const string WorkerName = "enforcement-to-policy-feedback";

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly EnforcementToPolicyFeedbackHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<EnforcementToPolicyFeedbackWorker>? _logger;

    // R2.A.3d Phase B: retry-tier escalation wiring. Note the handler
    // itself already catches CircuitBreakerOpenException (fire-and-forget
    // bridge), so escalation here covers non-breaker handler-throws —
    // deserialize-side faults + handler internals unrelated to the
    // Kafka-producer breaker.
    private readonly IProducer<string, string>? _producer;
    private readonly TopicNameResolver? _topicNameResolver;
    private readonly RetryTierOptions? _retryOptions;
    private readonly IRandomProvider? _randomProvider;
    private readonly ICircuitBreaker? _kafkaBreaker;

    public EnforcementToPolicyFeedbackWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        EnforcementToPolicyFeedbackHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<EnforcementToPolicyFeedbackWorker>? logger = null,
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
            // R2.E.1 / R-CONSUMER-REBALANCE-COOPERATIVE-STICKY-01
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
        };

        // R2.E.1 / R-CONSUMER-REBALANCE-01: rebalance observability wiring.
        var consumerBuilder = new ConsumerBuilder<string, string>(config);
        KafkaRebalanceObservability.Attach(consumerBuilder, SourceTopic, WorkerName, _logger);
        using var consumer = consumerBuilder.Build();
        consumer.Subscribe(SourceTopic);

        _logger?.LogInformation(
            "EnforcementToPolicyFeedbackWorker subscribed to {Topic} under consumer group {Group}.",
            SourceTopic, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                    continue;

                KafkaLagObservability.Record(consumer, result, WorkerName, SourceTopic);

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

                // R2.A.3d Phase B / R-RETRY-CONSUMER-INTEGRATION-02
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
                _logger?.LogError(ex, "EnforcementToPolicyFeedbackWorker iteration failed outside the retry-escalate path; will continue.");
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
                "EnforcementToPolicyFeedbackWorker handler failed on {Topic} p{Partition} o{Offset}; retry tier not wired, will redeliver.",
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
                "EnforcementToPolicyFeedbackWorker handler failed on {Topic} p{Partition} o{Offset}; retry-escalate blocked by breaker '{Breaker}' ({RetryAfter}s); will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value,
                breakerEx.BreakerName, breakerEx.RetryAfterSeconds);
        }
        catch (Exception escalationEx)
        {
            _logger?.LogError(escalationEx,
                "EnforcementToPolicyFeedbackWorker retry escalation failed on {Topic} p{Partition} o{Offset} after handler exception {HandlerException}; will redeliver.",
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
