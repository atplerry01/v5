using System.Diagnostics.Metrics;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
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
        TimeSpan? pollTimeout = null)
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

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
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
    }
}
