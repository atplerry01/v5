using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Phase 8 B4 — BackgroundService that subscribes to the sanction event
/// topic and routes each envelope through
/// <see cref="SanctionActivationEnforcementHandler"/>. Mirrors
/// <see cref="PayoutFailureCompensationWorker"/> in shape so failure
/// and commit semantics are consistent across the reactor surface.
///
/// Subscribed topic:
///   - whyce.economic.enforcement.sanction.events   (source of SanctionActivatedEvent)
///
/// Dedicated consumer group isolates commit progress from the sanction
/// projection and other consumers. Handler exceptions leave the offset
/// un-committed so the message is redelivered after restart; the
/// handler's envelope-level idempotency claim guarantees a second
/// attempt never produces a duplicate enforcement aggregate.
/// </summary>
public sealed class SanctionActivationEnforcementWorker : BackgroundService
{
    private static readonly string[] Topics =
    [
        "whyce.economic.enforcement.sanction.events"
    ];

    private const string ConsumerGroup = "whyce.saga.sanction-activation-enforcement";
    private const string WorkerName = "sanction-activation-enforcement"; // R2.E.1 rebalance-metric tag

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly SanctionActivationEnforcementHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<SanctionActivationEnforcementWorker>? _logger;

    // R2.A.3d Phase B: retry-tier escalation wiring. When all five are
    // supplied, handler-throw paths route through KafkaRetryEscalator
    // instead of falling to unbounded Kafka redelivery. Optional so
    // legacy test harnesses that construct the worker without the
    // retry tier continue to compile; null branch preserves
    // pre-R2.A.3d behaviour (log + no-commit + Kafka redeliver).
    private readonly IProducer<string, string>? _producer;
    private readonly TopicNameResolver? _topicNameResolver;
    private readonly RetryTierOptions? _retryOptions;
    private readonly IRandomProvider? _randomProvider;
    private readonly ICircuitBreaker? _kafkaBreaker;

    public SanctionActivationEnforcementWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        SanctionActivationEnforcementHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<SanctionActivationEnforcementWorker>? logger = null,
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
            "SanctionActivationEnforcementWorker subscribed to {TopicCount} topic(s) under consumer group {Group}.",
            Topics.Length, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                    continue;

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
                        "SanctionActivationEnforcementWorker skipping malformed message on {Topic}: missing or unparseable headers.",
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

                // R2.A.3d Phase B / R-RETRY-CONSUMER-INTEGRATION-02: wrap
                // the handler dispatch in a dedicated try/catch so handler
                // exceptions route through the retry escalator (bounded
                // attempts + exponential backoff) instead of looping
                // on Kafka redelivery. When the retry tier is not wired,
                // fall back to pre-Phase-B behaviour (log + no-commit).
                try
                {
                    // Background dispatch has no HTTP context — scope to a
                    // known system identity so the downstream dispatcher's
                    // identity middleware can satisfy actor / tenant lookups.
                    using (SystemIdentityScope.Begin(
                        "system/sanction-activation-enforcement", "system", "system"))
                    {
                        await _handler.HandleAsync(envelope, stoppingToken);
                    }
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
                _logger?.LogError(ex,
                    "SanctionActivationEnforcementWorker iteration failed outside the retry-escalate path; offset un-committed for redelivery.");
                // Offset is NOT committed — redelivery after restart will
                // retry; the handler's IIdempotencyStore claim ensures a
                // second attempt never produces a duplicate enforcement.
            }
        }

        consumer.Close();
    }

    /// <summary>
    /// R2.A.3d Phase B / R-RETRY-CONSUMER-INTEGRATION-02: shared
    /// escalate-or-redeliver post-handler-failure helper. When the
    /// retry tier is fully wired, route through
    /// <see cref="KafkaRetryEscalator.EscalateAsync"/> and commit
    /// the source offset on escalation success. When the escalator
    /// cannot publish (breaker Open, transport failure), do NOT
    /// commit — Kafka redelivers when the condition clears. When
    /// the retry tier is not wired (legacy test harness), fall back
    /// to pre-Phase-B behaviour (log + no-commit + Kafka redeliver).
    /// </summary>
    private async Task EscalateOrRedeliverAsync(
        IConsumer<string, string> consumer,
        ConsumeResult<string, string> result,
        Guid parsedEventId,
        Exception handlerEx,
        CancellationToken ct)
    {
        if (_producer is null
            || _topicNameResolver is null
            || _retryOptions is null
            || _randomProvider is null
            || _kafkaBreaker is null)
        {
            _logger?.LogError(handlerEx,
                "SanctionActivationEnforcementWorker handler failed on {Topic} p{Partition} o{Offset}; retry tier not wired, will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value);
            return;
        }

        var priorAttempt = RetryHeaders.ReadPriorAttemptCount(result.Message.Headers);

        try
        {
            await KafkaRetryEscalator.EscalateAsync(
                _producer,
                _topicNameResolver,
                result.Topic,
                result.Message,
                parsedEventId,
                $"{handlerEx.GetType().Name}: {handlerEx.Message}",
                priorAttempt,
                _retryOptions.MaxAttempts,
                _retryOptions.BaseBackoff,
                _retryOptions.MaxBackoff,
                _randomProvider,
                _clock,
                _kafkaBreaker,
                _logger,
                ct);
            consumer.Commit(result);
        }
        catch (CircuitBreakerOpenException breakerEx)
        {
            _logger?.LogWarning(
                "SanctionActivationEnforcementWorker handler failed on {Topic} p{Partition} o{Offset}; retry-escalate blocked by breaker '{Breaker}' ({RetryAfter}s); will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value,
                breakerEx.BreakerName, breakerEx.RetryAfterSeconds);
            // NO commit — Kafka redelivers when breaker recovers.
        }
        catch (Exception escalationEx)
        {
            _logger?.LogError(escalationEx,
                "SanctionActivationEnforcementWorker retry escalation failed on {Topic} p{Partition} o{Offset} after handler exception {HandlerException}; will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value, handlerEx.GetType().Name);
            // NO commit — Kafka redelivers.
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
