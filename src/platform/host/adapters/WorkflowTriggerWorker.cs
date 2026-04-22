using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Whycespace.Runtime.Security;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// BackgroundService that subscribes to the economic events topics and routes
/// each envelope through <see cref="WorkflowTriggerHandler"/>. Mirrors the
/// structure of <c>GenericKafkaProjectionConsumerWorker</c> but delegates to
/// a workflow-trigger handler instead of a projection registry.
///
/// Subscribed topics:
///   - whyce.economic.revenue.revenue.events
///   - whyce.economic.revenue.payout.events
///   - whyce.economic.revenue.distribution.events   (no-op in handler today)
///   - whyce.economic.vault.account.events          (no-op in handler today)
///
/// Malformed messages are skipped with a warning and offset-committed — no
/// dead-letter routing here because the projection worker for the same topic
/// already DLQs malformed messages. The trigger worker runs on a separate
/// consumer group, so a skipped trigger never loses data.
/// </summary>
public sealed class WorkflowTriggerWorker : BackgroundService
{
    private static readonly string[] Topics =
    [
        "whyce.economic.revenue.revenue.events",
        "whyce.economic.revenue.payout.events",
        "whyce.economic.revenue.distribution.events",
        "whyce.economic.vault.account.events"
    ];

    private const string ConsumerGroup = "whyce.trigger.economic";
    private const string WorkerName = "workflow-trigger"; // R2.E.1 rebalance-metric tag

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly WorkflowTriggerHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly ILogger<WorkflowTriggerWorker>? _logger;

    // R2.A.3d Phase B: retry-tier escalation wiring.
    private readonly IProducer<string, string>? _producer;
    private readonly TopicNameResolver? _topicNameResolver;
    private readonly RetryTierOptions? _retryOptions;
    private readonly IRandomProvider? _randomProvider;
    private readonly ICircuitBreaker? _kafkaBreaker;

    public WorkflowTriggerWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        WorkflowTriggerHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        ILogger<WorkflowTriggerWorker>? logger = null,
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
            "WorkflowTriggerWorker subscribed to {TopicCount} topics under consumer group {Group}.",
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
                    // D11: background dispatch carries no HTTP context. Scope to
                    // a known system identity so HttpCallerIdentityAccessor can
                    // satisfy SystemIntentDispatcher's actor/tenant lookups
                    // without violating WP-1 (HTTP requests still fail-closed
                    // because the scope is opt-in and AsyncLocal-bound).
                    using (SystemIdentityScope.Begin("system/workflow-trigger", "system", "system"))
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
                _logger?.LogError(ex, "WorkflowTriggerWorker iteration failed outside the retry-escalate path; will continue.");
                // Offset is NOT committed — message will be re-delivered after restart.
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
                "WorkflowTriggerWorker handler failed on {Topic} p{Partition} o{Offset}; retry tier not wired, will redeliver.",
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
                "WorkflowTriggerWorker handler failed on {Topic} p{Partition} o{Offset}; retry-escalate blocked by breaker '{Breaker}' ({RetryAfter}s); will redeliver.",
                result.Topic, result.Partition.Value, result.Offset.Value,
                breakerEx.BreakerName, breakerEx.RetryAfterSeconds);
        }
        catch (Exception escalationEx)
        {
            _logger?.LogError(escalationEx,
                "WorkflowTriggerWorker retry escalation failed on {Topic} p{Partition} o{Offset} after handler exception {HandlerException}; will redeliver.",
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
