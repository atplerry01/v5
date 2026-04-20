using System.Diagnostics.Metrics;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

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
    public static readonly Meter Meter = new("Whycespace.Projection.Consumer", "1.0");
    private static readonly Counter<long> ConsumedCounter = Meter.CreateCounter<long>("consumer.consumed");
    private static readonly Counter<long> DlqRoutedCounter = Meter.CreateCounter<long>("consumer.dlq_routed");
    private static readonly Counter<long> HandlerInvokedCounter = Meter.CreateCounter<long>("consumer.handler_invoked");
    // phase1.5-S5.2.2 / KC-3 (DLQ-OBSERVABILITY-01): consumer-side
    // DLQ publish failure counter. Pre-KC-3 the catch block in
    // PublishToDeadletterAsync swallowed exceptions with only a log
    // line, so a broken DLQ topic dropped malformed messages without
    // any operator-visible metric. KC-3 closes that gap by tagging
    // every catch with the exception type as `reason`. Low
    // cardinality (one tag value per .NET exception type observed).
    private static readonly Counter<long> DlqPublishFailedCounter =
        Meter.CreateCounter<long>("consumer.dlq_publish_failed");

    // phase1.5-S5.2.1 / PC-7 (PROJECTION-LAG-01): projection lag
    // histogram. Records, after every successful projection write, the
    // observed delay between the broker-recorded message timestamp and
    // the wall time at which the projection write completed.
    //
    // Definition (stable, documented):
    //
    //     projection.lag_seconds
    //         = (clock.UtcNow - message.Timestamp.UtcDateTime).TotalSeconds
    //
    //     measured at the moment the projection write returns, where
    //         message.Timestamp = the broker-assigned CreateTime of the
    //                             Kafka record (durable, set by the
    //                             producer or broker, not by the
    //                             consumer loop).
    //
    // Why this signal: it reflects the actual staleness a read-side
    // caller would see for the just-projected row — not consumer-loop
    // activity, not envelope construction time. A read-after-write
    // observer would see data exactly this old. A non-zero, growing
    // lag is the canonical "projection is falling behind" signal.
    //
    // Tag is `topic` only — one worker per topic in the current
    // architecture, so the topic is a 1:1 stable identity for the
    // projection without exploding cardinality.
    private static readonly Histogram<double> ProjectionLagSeconds =
        Meter.CreateHistogram<double>("projection.lag_seconds", unit: "s");

    private readonly string _kafkaBootstrapServers;
    private readonly string _topic;
    private readonly string _consumerGroup;
    private readonly EventDeserializer _deserializer;
    private readonly ProjectionRegistry _projectionRegistry;
    private readonly IPostgresProjectionWriter _writer;
    private readonly IClock _clock;
    private readonly ILogger<GenericKafkaProjectionConsumerWorker>? _logger;
    private readonly TimeSpan _pollTimeout;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly IWorkerLivenessRegistry _liveness;
    // R2.A.3c / R-DLQ-STORE-CONSUMER-MIRROR-01: optional durable mirror for
    // Kafka `.deadletter` publishes. When registered (host composition,
    // PostgresInfrastructureModule) every poison message is recorded to
    // `dead_letter_entries` after the Kafka ack. Best-effort + non-blocking
    // — store-write failure does not abort the consumer. Null in legacy
    // tests → Kafka-only behaviour, no regression.
    private readonly IDeadLetterStore? _deadLetterStore;

    // R2.A.D.3b / R-KAFKA-BREAKER-01: shared "kafka-producer" breaker. Wraps
    // the DLQ publish ProduceAsync. Optional for backwards compat with tests
    // that don't register the breaker.
    private readonly ICircuitBreaker? _kafkaBreaker;

    // R2.A.3d / R-RETRY-CONSUMER-INTEGRATION-01: retry tier escalation.
    // When all four are supplied, handler-throw paths route through
    // KafkaRetryEscalator.EscalateAsync (into `.retry` for attempts
    // within budget; `.deadletter` when exhausted). When any is null,
    // pre-R2.A.3d fallback: log + no-commit + rely on Kafka redelivery.
    private readonly TopicNameResolver? _topicNameResolver;
    private readonly RetryTierOptions? _retryOptions;
    private readonly IRandomProvider? _randomProvider;

    // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): canonical worker
    // name reported into the IWorkerLivenessRegistry after each loop
    // iteration that returns from the consume/handle path without an
    // escaping exception. Multiple per-topic worker instances all
    // report under the same canonical name by design — HC-5 keeps
    // the taxonomy low-cardinality.
    private const string WorkerName = "projection-consumer";

    // phase1.5-S5.2.1 / PC-6 (KAFKA-CONSUMER-CONFIG-01): the worker now
    // takes a declared KafkaConsumerOptions and applies it to the
    // ConsumerConfig built in ExecuteAsync. The sequential
    // consume → handle → commit shape, the per-message commit, and the
    // DLQ routing are all unchanged — only the buffering and session
    // parameters move from incidental librdkafka defaults to declared
    // configuration.
    public GenericKafkaProjectionConsumerWorker(
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        EventDeserializer deserializer,
        ProjectionRegistry projectionRegistry,
        IPostgresProjectionWriter writer,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        IWorkerLivenessRegistry liveness,
        ILogger<GenericKafkaProjectionConsumerWorker>? logger = null,
        TimeSpan? pollTimeout = null,
        IDeadLetterStore? deadLetterStore = null,
        ICircuitBreaker? kafkaBreaker = null,
        TopicNameResolver? topicNameResolver = null,
        RetryTierOptions? retryOptions = null,
        IRandomProvider? randomProvider = null)
    {
        ArgumentNullException.ThrowIfNull(consumerOptions);
        ArgumentNullException.ThrowIfNull(liveness);
        if (consumerOptions.QueuedMaxMessagesKbytes < 1)
            throw new ArgumentOutOfRangeException(
                nameof(consumerOptions), consumerOptions.QueuedMaxMessagesKbytes,
                "KafkaConsumerOptions.QueuedMaxMessagesKbytes must be at least 1.");
        if (consumerOptions.FetchMessageMaxBytes < 1024)
            throw new ArgumentOutOfRangeException(
                nameof(consumerOptions), consumerOptions.FetchMessageMaxBytes,
                "KafkaConsumerOptions.FetchMessageMaxBytes must be at least 1024.");
        if (consumerOptions.MaxPollIntervalMs < 1000)
            throw new ArgumentOutOfRangeException(
                nameof(consumerOptions), consumerOptions.MaxPollIntervalMs,
                "KafkaConsumerOptions.MaxPollIntervalMs must be at least 1000.");
        if (consumerOptions.SessionTimeoutMs < 1000)
            throw new ArgumentOutOfRangeException(
                nameof(consumerOptions), consumerOptions.SessionTimeoutMs,
                "KafkaConsumerOptions.SessionTimeoutMs must be at least 1000.");

        _kafkaBootstrapServers = kafkaBootstrapServers;
        _topic = topic;
        _consumerGroup = consumerGroup;
        _deserializer = deserializer;
        _projectionRegistry = projectionRegistry;
        _writer = writer;
        _clock = clock;
        _consumerOptions = consumerOptions;
        _liveness = liveness;
        _logger = logger;
        _pollTimeout = pollTimeout ?? TimeSpan.FromSeconds(1);
        _deadLetterStore = deadLetterStore;
        _kafkaBreaker = kafkaBreaker;
        _topicNameResolver = topicNameResolver;
        _retryOptions = retryOptions;
        _randomProvider = randomProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // phase1.5-S5.2.1 / PC-6 (KAFKA-CONSUMER-CONFIG-01): every
        // load-bearing buffering / session / poll parameter is now
        // declared via KafkaConsumerOptions. The four explicit
        // assignments below replace silent inheritance of the
        // librdkafka defaults (most importantly the ~1 GiB
        // queued.max.messages.kbytes that Step B P-B6 flagged).
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = _consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            QueuedMaxMessagesKbytes = _consumerOptions.QueuedMaxMessagesKbytes,
            // KafkaConsumerOptions.FetchMessageMaxBytes is the Phase
            // 1.5 canonical name; in Confluent.Kafka 2.x the matching
            // ConsumerConfig property is MessageMaxBytes (librdkafka
            // message.max.bytes — the per-message ceiling). The
            // property name moved; the semantic is identical.
            MessageMaxBytes = _consumerOptions.FetchMessageMaxBytes,
            MaxPollIntervalMs = _consumerOptions.MaxPollIntervalMs,
            SessionTimeoutMs = _consumerOptions.SessionTimeoutMs,
            // R2.E.1 / R-CONSUMER-REBALANCE-COOPERATIVE-STICKY-01:
            // cooperative-sticky reassigns only the delta across
            // rebalance events so instances that stay in the consumer
            // group keep their existing partitions — cuts rebalance
            // blast radius compared to the default Range / RoundRobin
            // strategies which revoke-and-reassign all partitions.
            // Safe here because per-message consumer.Commit(result) is
            // our canonical commit pattern; no cross-message batch
            // buffer needs different flush semantics between strategies.
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
        };

        // phase1.5-S5.2.1 / PC-6: log the applied prefetch/session
        // envelope at startup so an operator can confirm the declared
        // configuration is in effect without having to inspect the
        // Confluent.Kafka client state. Single line per worker
        // instance, low cardinality, no per-message overhead.
        _logger?.LogInformation(
            "Kafka consumer config applied for {Topic}: " +
            "QueuedMaxMessagesKbytes={QueuedMaxMessagesKbytes}, " +
            "FetchMessageMaxBytes={FetchMessageMaxBytes}, " +
            "MaxPollIntervalMs={MaxPollIntervalMs}, " +
            "SessionTimeoutMs={SessionTimeoutMs}",
            _topic,
            _consumerOptions.QueuedMaxMessagesKbytes,
            _consumerOptions.FetchMessageMaxBytes,
            _consumerOptions.MaxPollIntervalMs,
            _consumerOptions.SessionTimeoutMs);

        // phase1-gate-S3: deadletter topic derived from the source topic by
        // replacing the trailing `.events` segment with `.deadletter`. Topics
        // are pre-provisioned by infrastructure/event-fabric/kafka/create-topics.sh.
        var deadletterTopic = _topic.EndsWith(".events")
            ? string.Concat(_topic.AsSpan(0, _topic.Length - ".events".Length), ".deadletter")
            : _topic + ".deadletter";

        var producerConfig = new ProducerConfig { BootstrapServers = _kafkaBootstrapServers };
        using var deadletterProducer = new ProducerBuilder<string, string>(producerConfig).Build();

        // R2.E.1 / R-CONSUMER-REBALANCE-01: rebalance-event handlers wired
        // via the canonical KafkaRebalanceObservability helper before
        // Build() so every assign/revoke/lost transition is observable
        // on the Whycespace.Kafka.Consumer meter.
        var consumerBuilder = new ConsumerBuilder<string, string>(config);
        KafkaRebalanceObservability.Attach(consumerBuilder, _topic, WorkerName, _logger);
        using var consumer = consumerBuilder.Build();
        consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(_pollTimeout);
                if (result is null)
                {
                    // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): an
                    // empty poll is a successful loop iteration — the
                    // consumer is healthy, the topic is just idle.
                    // Without this an idle topic would falsely flip
                    // the worker to "silent" after MaxSilenceSeconds.
                    _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
                    continue;
                }

                // R2.E.2 / R-CONSUMER-LAG-01: record per-partition lag
                // as early as possible after the Consume — poison
                // messages / DLQ-routed messages still emit lag before
                // commit because the signal means "we saw this offset;
                // how far behind are we?" regardless of routing outcome.
                KafkaLagObservability.Record(consumer, result, WorkerName, _topic);

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
                        "absent event-type header",
                        RuntimeFailureCategory.PoisonMessage,
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }
                if (eventType.Length == 0)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        "empty event-type header (present but blank)",
                        RuntimeFailureCategory.PoisonMessage,
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }

                if (eventIdHeader is null || aggregateIdHeader is null)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        $"absent event-id/aggregate-id headers (event-type={eventType})",
                        RuntimeFailureCategory.PoisonMessage,
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }
                if (eventIdHeader.Length == 0 || aggregateIdHeader.Length == 0)
                {
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message,
                        $"empty event-id/aggregate-id headers (present but blank, event-type={eventType})",
                        RuntimeFailureCategory.PoisonMessage,
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
                        RuntimeFailureCategory.PoisonMessage,
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }

                ConsumedCounter.Add(1, new KeyValuePair<string, object?>("topic", _topic));
                _logger?.LogDebug("Consumed {EventType} from {Topic}", eventType, _topic);

                // E5.Z/Projection-Pipeline-Correction: poisoned-payload isolation.
                //
                // DeserializeInbound (and downstream handler JSON work) can throw
                // for a poisoned message whose on-wire shape no longer matches the
                // registered inbound schema (e.g. historical pre-fix events on an
                // upgraded topic). Before this pass the generic `catch (Exception)`
                // at the bottom of the poll loop logged the failure and slept 1s
                // WITHOUT committing — so Kafka redelivered the same offset on the
                // next poll and the consumer livelocked, blocking every valid
                // message queued behind it on that partition.
                //
                // The fix is a narrow nested try around the deserialize+project
                // block. On the two well-defined poison classes (JSON shape
                // mismatch, schema-registry mismatch) we publish the *original*
                // Kafka record to the `.deadletter` topic (reason header carries
                // topic/partition/offset/event-type for forensic replay), commit
                // the source offset, and `continue` to the next message. Valid
                // messages after the poisoned one are processed normally and in
                // order — Kafka's per-partition ordering is preserved because the
                // commit advances the offset by exactly one bad record. Any other
                // exception type is intentionally NOT caught here and falls through
                // to the outer generic catch so its back-off behaviour is unchanged
                // (a handler DB outage must not silently DLQ application state).
                object @event;
                try
                {
                    @event = _deserializer.DeserializeInbound(eventType, rawPayload);
                }
                catch (Exception ex) when (IsPoisonedPayload(ex))
                {
                    var reason = $"poisoned payload (event-type={eventType}, partition={result.Partition.Value}, offset={result.Offset.Value}): {ex.GetType().Name}: {ex.Message}";
                    _logger?.LogError(ex,
                        "Poisoned payload on {Topic} p{Partition} o{Offset} event-type={EventType}; routing to DLQ and committing offset",
                        _topic, result.Partition.Value, result.Offset.Value, eventType);
                    await PublishToDeadletterAsync(
                        deadletterProducer, deadletterTopic, result.Message, reason,
                        RuntimeFailureCategory.PoisonMessage,
                        stoppingToken);
                    consumer.Commit(result);
                    continue;
                }

                var causationIdHeader = ExtractHeader(result.Message.Headers, "causation-id");
                var envelope = new EventEnvelope
                {
                    EventId = parsedEventId,
                    AggregateId = parsedAggregateId,
                    CorrelationId = Guid.TryParse(correlationId, out var cid) ? cid : Guid.Empty,
                    CausationId = Guid.TryParse(causationIdHeader, out var causId) ? causId : Guid.Empty,
                    EventType = eventType,
                    EventName = eventType,
                    EventVersion = EventVersion.Default,
                    SchemaHash = string.Empty,
                    Payload = @event,
                    ExecutionHash = string.Empty,
                    PolicyHash = string.Empty,
                    Timestamp = _clock.UtcNow
                };

                // R2.A.3d / R-RETRY-CONSUMER-INTEGRATION-01: handler +
                // writer wrapped in a dedicated try/catch so transient
                // handler failures route through the retry tier
                // escalator (bounded attempts with exponential backoff)
                // instead of hot-looping on Kafka redelivery or going
                // straight to `.deadletter`. When the retry tier is not
                // wired (legacy composition, test harnesses), falls back
                // to pre-R2.A.3d behaviour: do NOT commit, log + sleep,
                // let Kafka redeliver (unbounded attempts).
                try
                {
                    var handlers = _projectionRegistry.ResolveHandlers(eventType).ToList();
                    foreach (var handler in handlers)
                    {
                        HandlerInvokedCounter.Add(1,
                            new KeyValuePair<string, object?>("event_type", eventType),
                            new KeyValuePair<string, object?>("handler", handler.GetType().Name));
                        // phase1.5-S5.2.3 / TC-6 (PROJECTION-CT-CONTRACT-01):
                        // forward the worker stoppingToken into the handler so
                        // a hung handler is unblocked at the database round-trip
                        // when the host is shutting down, instead of waiting for
                        // Kafka poll/session limits to intervene. No per-handler
                        // CTS / declared timeout is introduced in this pass —
                        // that is a future workstream once the contract carries
                        // the token end-to-end.
                        await handler.HandleAsync(envelope, stoppingToken);
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
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Host shutdown — caller-driven cancellation. Re-throw
                    // so the outer catch handles loop exit.
                    throw;
                }
                catch (Exception handlerEx)
                {
                    if (_topicNameResolver is not null
                        && _retryOptions is not null
                        && _randomProvider is not null
                        && _kafkaBreaker is not null)
                    {
                        // R-RETRY-CONSUMER-INTEGRATION-01: escalate via
                        // the retry tier. Read prior attempt count from
                        // incoming headers so a message that has already
                        // bounced through `.retry` increments correctly.
                        var priorAttempt = RetryHeaders.ReadPriorAttemptCount(result.Message.Headers);

                        try
                        {
                            await KafkaRetryEscalator.EscalateAsync(
                                deadletterProducer,
                                _topicNameResolver,
                                _topic,
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
                                stoppingToken);

                            // Escalation succeeded — commit source offset.
                            // The message now lives on either `.retry` (if
                            // within attempt budget) or `.deadletter` (if
                            // exhausted); either way, the source offset is
                            // safe to advance.
                            consumer.Commit(result);
                        }
                        catch (CircuitBreakerOpenException breakerEx)
                        {
                            // Kafka producer breaker Open during escalate.
                            // Do NOT commit — Kafka redelivers when breaker
                            // recovers. Matches R-KAFKA-BREAKER-OPEN-BEHAVIOR-01
                            // outbox posture (skip tick, preserve row).
                            _logger?.LogWarning(
                                "Handler failed on {Topic} p{Partition} o{Offset}; retry-escalate blocked by breaker '{Breaker}' ({RetryAfter}s); will redeliver.",
                                _topic, result.Partition.Value, result.Offset.Value,
                                breakerEx.BreakerName, breakerEx.RetryAfterSeconds);
                        }
                        catch (Exception escalationEx)
                        {
                            _logger?.LogError(escalationEx,
                                "Retry escalation failed for {Topic} p{Partition} o{Offset} after handler exception {HandlerException}; will redeliver.",
                                _topic, result.Partition.Value, result.Offset.Value, handlerEx.GetType().Name);
                        }
                    }
                    else
                    {
                        // Pre-R2.A.3d fallback: retry tier not wired. Do
                        // NOT commit — rely on Kafka redelivery (unbounded
                        // attempts). The outer catch-Exception block below
                        // will handle logging + back-off.
                        _logger?.LogError(handlerEx,
                            "Handler failed on {Topic} p{Partition} o{Offset}; retry tier not wired, will redeliver.",
                            _topic, result.Partition.Value, result.Offset.Value);
                    }
                    // Skip the success-path commit + lag recording below.
                    continue;
                }

                // phase1.5-S5.2.1 / PC-7 (PROJECTION-LAG-01): record
                // projection lag immediately after the write returns.
                // Both branches converge here so the lag covers
                // domain-handler writes and generic raw-payload writes
                // alike. Recorded once per successfully projected
                // message; never recorded on DLQ-routed messages
                // (which never reach the read side).
                var brokerTimestamp = result.Message.Timestamp.UtcDateTime;
                var lagSeconds = (_clock.UtcNow.UtcDateTime - brokerTimestamp).TotalSeconds;
                ProjectionLagSeconds.Record(lagSeconds,
                    new KeyValuePair<string, object?>("topic", _topic));

                consumer.Commit(result);

                // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): record
                // a successful iteration ONLY on the success path —
                // never inside a catch block. DLQ-routed messages
                // also reach this point because their `continue`
                // happens before this line, so they do NOT count as
                // a "successful loop iteration that returned from
                // consume/handle". An idle empty-poll loop is still
                // covered by the `result is null` continue above —
                // see comment below for the empty-poll branch.
                _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
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
    /// E5.Z/Projection-Pipeline-Correction: recognises the narrow set of
    /// exceptions that mean "the on-wire payload cannot be mapped to a
    /// registered inbound schema" — i.e. a poisoned message that no amount
    /// of retry will fix. Matching is intentionally tight:
    ///
    ///   - <see cref="System.Text.Json.JsonException"/> covers shape
    ///     mismatches (e.g. historical pre-fix events whose primitives are
    ///     still nested domain value-objects).
    ///   - <see cref="InvalidOperationException"/> is matched ONLY when the
    ///     message text corresponds to one of the two sentinels thrown by
    ///     <c>EventDeserializer.DeserializeInbound</c>:
    ///       * "EventSchemaRegistry has no InboundEventType registered"
    ///       * "Failed to deserialize inbound event"
    ///     Any other InvalidOperationException (DB layer, etc.) is deliberately
    ///     NOT treated as poisoned so it continues to propagate to the outer
    ///     back-off catch and does not silently DLQ application-level state.
    /// </summary>
    private static bool IsPoisonedPayload(Exception ex) => ex switch
    {
        System.Text.Json.JsonException => true,
        InvalidOperationException ioe when
            ioe.Message.StartsWith("EventSchemaRegistry has no InboundEventType registered", StringComparison.Ordinal)
            || ioe.Message.StartsWith("Failed to deserialize inbound event", StringComparison.Ordinal) => true,
        _ => false,
    };

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

    // R2.A.3d Phase B: ReadPriorAttemptCount promoted to
    // RetryHeaders.ReadPriorAttemptCount so all 11 consumer workers share
    // the same header-parse discipline.

    /// <summary>
    /// phase1-gate-S3: publishes a malformed inbound message to the deadletter
    /// topic preserving original key/value/headers and adding diagnostic
    /// `dlq-reason` and `dlq-source-topic` headers.
    ///
    /// phase1.5 S0-3 (K-DLQ-001): a DLQ publish failure MUST propagate so the
    /// caller does NOT commit the source offset. Pre-S0-3 this method swallowed
    /// the exception and the caller committed unconditionally, which silently
    /// dropped messages on a broken DLQ topic. Now the publish-failed counter
    /// is bumped, the failure is logged, and the exception is re-thrown so the
    /// outer consume loop's catch handles back-off and Kafka re-delivers the
    /// uncommitted message on the next poll.
    /// </summary>
    private async Task PublishToDeadletterAsync(
        IProducer<string, string> producer,
        string deadletterTopic,
        Message<string, string> original,
        string reason,
        RuntimeFailureCategory category,
        CancellationToken ct)
    {
        var aggregateIdHeader = ExtractHeader(original.Headers ?? new Headers(), "aggregate-id");
        var eventIdHeader = ExtractHeader(original.Headers ?? new Headers(), "event-id");
        var correlationHeader = ExtractHeader(original.Headers ?? new Headers(), "correlation-id");
        var causationHeader = ExtractHeader(original.Headers ?? new Headers(), "causation-id");
        var eventTypeHeader = ExtractHeader(original.Headers ?? new Headers(), "event-type");

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
            // R2.A.3c / R-DLQ-CATEGORY-POISON-01: surface the canonical
            // category on the Kafka record too, so downstream DLQ
            // consumers can route by category without parsing the reason.
            headers.Add("dlq-category",     Encoding.UTF8.GetBytes(category.ToString()));

            // phase1-gate-H7a: enforce per-aggregate Kafka ordering. Prefer the
            // aggregate-id header (set by KafkaOutboxPublisher); fall back to the
            // original key if the header is missing or unparseable so that
            // malformed-header DLQ paths are still routable.
            // phase1.6-S2.3: ExtractHeader now returns null for absent
            // headers (vs empty for present-but-blank). Treat both as
            // "not usable as a partition key" and fall through to
            // original.Key so the DLQ row still routes to a partition.
            var dlqKey = !string.IsNullOrEmpty(aggregateIdHeader) ? aggregateIdHeader : original.Key;
            var dlqMessage = new Message<string, string>
            {
                Key = dlqKey,
                Value = original.Value,
                Headers = headers
            };

            // R2.A.D.3b / R-KAFKA-BREAKER-01: DLQ publish flows through the
            // shared "kafka-producer" breaker. Open-state re-throws a
            // CircuitBreakerOpenException; the existing outer catch of this
            // method re-throws to the consume loop, which honors K-DLQ-001
            // (never commit the source offset on DLQ publish failure) so
            // Kafka re-delivers on the next poll. No behaviour change at
            // the offset-commit boundary.
            if (_kafkaBreaker is null)
            {
                await producer.ProduceAsync(deadletterTopic, dlqMessage, ct);
            }
            else
            {
                await _kafkaBreaker.ExecuteAsync(async c =>
                    await producer.ProduceAsync(deadletterTopic, dlqMessage, c), ct);
            }
            DlqRoutedCounter.Add(1,
                new KeyValuePair<string, object?>("source_topic", _topic),
                new KeyValuePair<string, object?>("reason", reason),
                new KeyValuePair<string, object?>("category", category.ToString()));
            _logger?.LogWarning(
                "Routed message from {SourceTopic} to {DeadletterTopic} (category={Category}): {Reason}",
                _topic, deadletterTopic, category, reason);
        }
        catch (Exception ex)
        {
            // phase1.5 S0-3 (K-DLQ-001): bump the publish-failure counter
            // and log, then re-throw. The outer consume-loop catch will
            // back off, and because the source offset has NOT been
            // committed (every call site commits only on the line AFTER
            // this method returns) Kafka will re-deliver on the next poll.
            DlqPublishFailedCounter.Add(1,
                new KeyValuePair<string, object?>("source_topic", _topic),
                new KeyValuePair<string, object?>("reason", ex.GetType().Name));
            _logger?.LogCritical(ex,
                "DLQ publish failed for {DeadletterTopic} — refusing to commit source offset",
                deadletterTopic);
            throw;
        }

        // R2.A.3c / R-DLQ-STORE-CONSUMER-MIRROR-01: mirror to durable store
        // so operator inspection + re-drive + retention queries can see
        // this entry. Best-effort + non-blocking — store failure is
        // caught, logged, and swallowed. The Kafka `.deadletter` record
        // above is the authoritative path; the store is the queryable view.
        // If the store rejects the write, the source offset has ALREADY
        // been acknowledged upstream by the time we return (every caller
        // commits on the line after this method returns), so a store
        // failure can only degrade the operator-inspection surface, not
        // lose the message.
        if (_deadLetterStore is null) return;

        try
        {
            var eventId = Guid.TryParse(eventIdHeader, out var parsedEventId) && parsedEventId != Guid.Empty
                ? parsedEventId
                // No event-id header (or unparseable) — derive a deterministic id from
                // the source topic + value-hash so the DLQ store still has a primary
                // key, idempotent on retry of the same poison record.
                : DeriveFallbackEventId(original);

            Guid? causationId = Guid.TryParse(causationHeader, out var parsedCausation)
                ? parsedCausation
                : null;
            var correlationId = Guid.TryParse(correlationHeader, out var parsedCorrelation)
                ? parsedCorrelation
                : Guid.Empty;

            var entry = new DeadLetterEntry
            {
                EventId = eventId,
                SourceTopic = _topic,
                EventType = eventTypeHeader ?? "unknown",
                CorrelationId = correlationId,
                CausationId = causationId,
                EnqueuedAt = _clock.UtcNow,
                FailureCategory = category,
                LastError = reason,
                AttemptCount = 0, // poison routes go direct to DLQ per R-TOPIC-TIER-01
                Payload = Encoding.UTF8.GetBytes(original.Value ?? string.Empty)
            };

            await _deadLetterStore.RecordAsync(entry, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "FAILED to mirror consumer-poison message from {SourceTopic} to IDeadLetterStore. Kafka deadletter has the record; operator inspection surface degraded but no data lost.",
                _topic);
        }
    }

    /// <summary>
    /// R2.A.3c — fallback EventId derivation for poison messages whose
    /// event-id header is absent or unparseable. SHA256 of the source
    /// topic + payload gives a deterministic key so the DLQ store remains
    /// idempotent on retry of the identical poison bytes.
    /// </summary>
    private static Guid DeriveFallbackEventId(Message<string, string> message)
    {
        var input = string.Concat(message.Key ?? string.Empty, "|", message.Value ?? string.Empty);
        Span<byte> hash = stackalloc byte[32];
        System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(input), hash);
        Span<byte> guidBytes = stackalloc byte[16];
        hash[..16].CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
