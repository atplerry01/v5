using System.Diagnostics.Metrics;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Npgsql;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// Background service that polls the Postgres outbox table and publishes
/// pending events to Kafka. Implements the transactional outbox pattern:
///   1. SELECT ... WHERE status = 'pending' ... FOR UPDATE SKIP LOCKED
///   2. Produce to Kafka using the topic stored in each outbox entry
///   3. UPDATE status = 'published' (per row)
///
/// Row-level isolation: a ProduceException on one row marks that row 'failed'
/// and continues with the rest of the batch. The publisher loop NEVER crashes
/// the host — all exceptions are caught and logged.
/// </summary>
public sealed class KafkaOutboxPublisher : BackgroundService
{
    // phase1-gate-S6: observability meter. Counters are exported via any
    // registered MeterListener (OTel, Prometheus exporter, dotnet-counters, ...).
    public static readonly Meter Meter = new("Whyce.Outbox", "1.0");
    private static readonly Counter<long> PublishedCounter    = Meter.CreateCounter<long>("outbox.published");
    private static readonly Counter<long> FailedCounter       = Meter.CreateCounter<long>("outbox.failed");
    private static readonly Counter<long> DeadletteredCounter = Meter.CreateCounter<long>("outbox.deadlettered");
    private static readonly Counter<long> DlqPublishedCounter = Meter.CreateCounter<long>("outbox.dlq_published");

    private readonly EventStoreDataSource _dataSource;
    private readonly IProducer<string, string> _producer;
    private readonly TopicNameResolver _topicNameResolver;
    private readonly TimeSpan _pollInterval;
    private readonly int _maxRetryCount;
    private readonly IWorkerLivenessRegistry _liveness;
    private readonly IClock _clock;
    private readonly ILogger<KafkaOutboxPublisher>? _logger;

    // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): canonical worker
    // name reported into the IWorkerLivenessRegistry after each
    // successful PublishBatchAsync cycle (no exception escaped).
    private const string WorkerName = "kafka-outbox-publisher";

    // phase1.6-S1.5 (OUTBOX-CONFIG-01): retry budget arrives as a typed
    // OutboxOptions record from the composition root, which reads it from
    // configuration. There is no longer a hardcoded constant or a default
    // parameter on this constructor — the option is required, and its
    // single source of truth is OutboxOptions.MaxRetry (which itself
    // carries a conservative default for the case where the config key
    // is unset).
    //
    // phase1.6-S1.6 (DLQ-RESOLVER-01): the dead-letter topic name is now
    // resolved through the canonical TopicNameResolver instead of inline
    // string manipulation. The resolver is required (not nullable, not
    // defaulted) so the publisher cannot construct a DLQ topic by any
    // other path.
    // phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): the publisher's polling
    // and retry-update transactions now flow through the declared
    // event-store NpgsqlDataSource. Polling cadence, batch size, and
    // retry semantics are unchanged.
    public KafkaOutboxPublisher(
        EventStoreDataSource dataSource,
        IProducer<string, string> producer,
        TopicNameResolver topicNameResolver,
        OutboxOptions options,
        IWorkerLivenessRegistry liveness,
        IClock clock,
        TimeSpan? pollInterval = null,
        ILogger<KafkaOutboxPublisher>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(topicNameResolver);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(liveness);
        ArgumentNullException.ThrowIfNull(clock);
        if (options.MaxRetry < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.MaxRetry,
                "OutboxOptions.MaxRetry must be at least 1.");

        _dataSource = dataSource;
        _producer = producer;
        _topicNameResolver = topicNameResolver;
        _pollInterval = pollInterval ?? TimeSpan.FromSeconds(1);
        _maxRetryCount = options.MaxRetry;
        _liveness = liveness;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Outer loop is unconditionally guarded so a transient DB / Kafka
        // failure can never escape and trigger BackgroundServiceExceptionBehavior.StopHost.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await PublishBatchAsync(stoppingToken);
                // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): record
                // a successful iteration ONLY after the batch returns
                // without an escaping exception. Recorded once per
                // outer loop tick — empty-batch ticks count as live
                // because the publisher genuinely completed a healthy
                // poll cycle.
                _liveness.RecordSuccess(WorkerName, _clock.UtcNow);
                if (published == 0)
                    await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "KafkaOutboxPublisher batch loop error; will retry after delay.");
                try { await Task.Delay(_pollInterval, stoppingToken); }
                catch (OperationCanceledException) { return; }
            }
        }
    }

    private async Task<int> PublishBatchAsync(CancellationToken ct)
    {
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName, ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        // Fetch pending + previously-failed-but-under-retry-budget rows with
        // SKIP LOCKED to allow concurrent workers. 'deadletter' rows are excluded.
        await using var selectCmd = new NpgsqlCommand(
            """
            SELECT id, correlation_id, event_id, aggregate_id, event_type, payload, topic, retry_count
            FROM outbox
            WHERE (status = 'pending')
               OR (status = 'failed'
                   AND retry_count < @max_retry
                   AND (next_retry_at IS NULL OR next_retry_at <= NOW()))
            ORDER BY created_at ASC
            LIMIT 100
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        selectCmd.Parameters.AddWithValue("max_retry", _maxRetryCount);

        var batch = new List<(Guid Id, Guid CorrelationId, Guid EventId, Guid AggregateId, string EventType, string Payload, string Topic, int RetryCount)>();
        await using (var reader = await selectCmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                batch.Add((
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.GetGuid(2),
                    reader.GetGuid(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6),
                    reader.GetInt32(7)));
            }
        }

        if (batch.Count == 0)
        {
            await tx.CommitAsync(ct);
            return 0;
        }

        var publishedCount = 0;

        foreach (var entry in batch)
        {
            // phase1-gate-S2: event_id and aggregate_id are now first-class outbox
            // columns (migration 004). No more JSON parsing of the payload at
            // publish time — headers come straight from the row.
            // phase1-gate-H7a: enforce per-aggregate Kafka ordering
            var message = new Message<string, string>
            {
                Key = entry.AggregateId.ToString(),
                Value = entry.Payload,
                Headers = new Headers
                {
                    { "event-id", System.Text.Encoding.UTF8.GetBytes(entry.EventId.ToString()) },
                    { "aggregate-id", System.Text.Encoding.UTF8.GetBytes(entry.AggregateId.ToString()) },
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(entry.EventType) },
                    { "correlation-id", System.Text.Encoding.UTF8.GetBytes(entry.CorrelationId.ToString()) }
                }
            };

            try
            {
                _logger?.LogDebug(
                    "Publishing outbox row {OutboxId} ({EventType}) to topic {Topic}",
                    entry.Id, entry.EventType, entry.Topic);

                await _producer.ProduceAsync(entry.Topic, message, ct);
                await MarkAsPublishedAsync(conn, tx, entry.Id, ct);
                publishedCount++;
                PublishedCounter.Add(1, new KeyValuePair<string, object?>("topic", entry.Topic));
            }
            catch (ProduceException<string, string> ex)
            {
                _logger?.LogError(
                    ex,
                    "Kafka produce failed for outbox row {OutboxId} on topic {Topic} (attempt {Attempt}/{Max}): {Reason}",
                    entry.Id, entry.Topic, entry.RetryCount + 1, _maxRetryCount, ex.Error.Reason);

                await RecordFailureAsync(conn, tx, entry.Id, entry.RetryCount, ex.Error.Reason, ct);
                await TryPublishToDeadletterAsync(entry, ex.Error.Reason, ct);
                continue; // NEVER break the batch on a single bad row.
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Unexpected error publishing outbox row {OutboxId} on topic {Topic} (attempt {Attempt}/{Max})",
                    entry.Id, entry.Topic, entry.RetryCount + 1, _maxRetryCount);

                await RecordFailureAsync(conn, tx, entry.Id, entry.RetryCount, ex.Message, ct);
                await TryPublishToDeadletterAsync(entry, ex.Message, ct);
                continue;
            }
        }

        await tx.CommitAsync(ct);
        return publishedCount;
    }

    private static async Task MarkAsPublishedAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid id, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(
            "UPDATE outbox SET status = 'published', published_at = NOW() WHERE id = @id",
            conn, tx);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task RecordFailureAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid id, int currentRetryCount, string reason, CancellationToken ct)
    {
        // Promote to 'deadletter' once retry budget exhausted; otherwise mark 'failed'
        // (which the next batch will pick up via the SELECT WHERE status='failed' AND retry_count<max).
        var nextAttempt = currentRetryCount + 1;
        var nextStatus = nextAttempt >= _maxRetryCount ? "deadletter" : "failed";

        // phase1-gate-S5: exponential backoff. next_retry_at gates re-selection
        // so failed rows don't get hammered on the next 1-sec poll. Schedule:
        //   1st fail → +1s,  2nd → +2s,  3rd → +4s,  4th → +8s ...
        // Capped at 5 minutes to bound worst-case latency. Computed server-side
        // via NOW() to honor $9 (no client clock).
        var backoffSeconds = (int)Math.Min(Math.Pow(2, nextAttempt - 1), 300);

        await using var cmd = new NpgsqlCommand(
            """
            UPDATE outbox
            SET status        = @status,
                retry_count   = retry_count + 1,
                last_error    = @err,
                next_retry_at = NOW() + (@backoff_seconds || ' seconds')::interval
            WHERE id = @id
            """,
            conn, tx);
        cmd.Parameters.AddWithValue("status", nextStatus);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("err", reason ?? string.Empty);
        cmd.Parameters.AddWithValue("backoff_seconds", backoffSeconds.ToString());
        await cmd.ExecuteNonQueryAsync(ct);

        FailedCounter.Add(1);
        if (nextStatus == "deadletter")
        {
            DeadletteredCounter.Add(1);
            _logger?.LogError(
                "Outbox row {OutboxId} promoted to deadletter after {Max} failed attempts. Last error: {Reason}",
                id, _maxRetryCount, reason);
        }
    }

    /// <summary>
    /// phase1-gate-S4: when an outbox row exhausts its retry budget, also
    /// publish the original payload to the corresponding `*.deadletter` Kafka
    /// topic with error/retry metadata headers. The DB is the source of
    /// truth (status='deadletter'), but downstream consumers/operators get
    /// real-time visibility via the deadletter topic.
    ///
    /// Failures here are caught and logged — DLQ publish must never crash
    /// the publisher loop ($12).
    /// </summary>
    private async Task TryPublishToDeadletterAsync(
        (Guid Id, Guid CorrelationId, Guid EventId, Guid AggregateId, string EventType, string Payload, string Topic, int RetryCount) entry,
        string reason,
        CancellationToken ct)
    {
        // Only DLQ-publish on the attempt that exhausts the retry budget.
        var willBeDeadlettered = (entry.RetryCount + 1) >= _maxRetryCount;
        if (!willBeDeadlettered) return;

        // phase1.6-S1.6 (DLQ-RESOLVER-01): route through the canonical
        // TopicNameResolver. The previous inline string manipulation is
        // removed; all DLQ naming is now owned by a single seam.
        var deadletterTopic = _topicNameResolver.ResolveDeadLetter(entry.Topic);

        try
        {
            // phase1-gate-H7a: enforce per-aggregate Kafka ordering
            var dlqMessage = new Message<string, string>
            {
                Key = entry.AggregateId.ToString(),
                Value = entry.Payload,
                Headers = new Headers
                {
                    { "event-id",        System.Text.Encoding.UTF8.GetBytes(entry.EventId.ToString()) },
                    { "aggregate-id",    System.Text.Encoding.UTF8.GetBytes(entry.AggregateId.ToString()) },
                    { "event-type",      System.Text.Encoding.UTF8.GetBytes(entry.EventType) },
                    { "correlation-id",  System.Text.Encoding.UTF8.GetBytes(entry.CorrelationId.ToString()) },
                    { "dlq-reason",      System.Text.Encoding.UTF8.GetBytes(reason ?? string.Empty) },
                    { "dlq-attempts",    System.Text.Encoding.UTF8.GetBytes((entry.RetryCount + 1).ToString()) },
                    { "dlq-source-topic", System.Text.Encoding.UTF8.GetBytes(entry.Topic) }
                }
            };

            await _producer.ProduceAsync(deadletterTopic, dlqMessage, ct);
            DlqPublishedCounter.Add(1, new KeyValuePair<string, object?>("topic", deadletterTopic));
            _logger?.LogError(
                "Outbox row {OutboxId} routed to deadletter topic {DeadletterTopic} after {Attempts} attempts: {Reason}",
                entry.Id, deadletterTopic, entry.RetryCount + 1, reason);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "FAILED to publish outbox row {OutboxId} to deadletter topic {DeadletterTopic}. DB row remains status='deadletter' for manual recovery.",
                entry.Id, deadletterTopic);
        }
    }

    public override void Dispose()
    {
        _producer.Dispose();
        base.Dispose();
    }
}
