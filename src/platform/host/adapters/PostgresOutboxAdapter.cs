using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed outbox. Events are persisted to the outbox table for
/// reliable at-least-once delivery. The KafkaOutboxPublisher polls this
/// table and relays events to Kafka.
/// </summary>
public sealed class PostgresOutboxAdapter : IOutbox
{
    private readonly EventStoreDataSource _dataSource;
    private readonly IOutboxDepthSnapshot _depthSnapshot;
    private readonly OutboxOptions _options;
    private readonly IClock _clock;

    // phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): connection lifecycle
    // moved to the declared event-store pool. The high-water-mark
    // refusal path (PC-3) and the INSERT loop are unchanged.
    //
    // phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01): IClock is
    // now constructor-injected so the freshness check at the refusal
    // seam consults the canonical Whycespace clock rather than
    // DateTime.UtcNow. Closes H19 (stale snapshot from a dead sampler
    // silently corrupts PC-3 admission decisions).
    public PostgresOutboxAdapter(
        EventStoreDataSource dataSource,
        IOutboxDepthSnapshot depthSnapshot,
        OutboxOptions options,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(depthSnapshot);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        if (options.HighWaterMark < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.HighWaterMark,
                "OutboxOptions.HighWaterMark must be at least 1.");
        if (options.SnapshotMaxAgeSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.SnapshotMaxAgeSeconds,
                "OutboxOptions.SnapshotMaxAgeSeconds must be at least 1.");

        _dataSource = dataSource;
        _depthSnapshot = depthSnapshot;
        _options = options;
        _clock = clock;
    }

    public async Task EnqueueAsync(
        Guid correlationId,
        IReadOnlyList<object> events,
        string topic,
        CancellationToken cancellationToken = default)
    {
        // phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): high-water-mark
        // refusal. Read the latest sampled depth from the shared
        // snapshot — never a per-enqueue COUNT(*). Until the sampler
        // has produced its first observation we admit unconditionally
        // (the initial-startup window is bounded by
        // OutboxOptions.SamplingIntervalSeconds and the snapshot is
        // primed on the first sampler tick at host start). Once an
        // observation exists, depth ≥ HighWaterMark throws the typed
        // RETRYABLE REFUSAL exception which the API edge maps to 503 +
        // Retry-After.
        // phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01):
        // fail-safe stale-snapshot refusal. Closes H19 — pre-HC-1 a
        // dead OutboxDepthSampler froze the snapshot at its last
        // value, and this comparator would treat that frozen value
        // as authoritative forever (silently admitting under a
        // stale below-watermark observation, or silently refusing
        // under a stale above-watermark one). The freshness check
        // runs BEFORE the high-water-mark comparator and uses the
        // same canonical refusal family — only the Reason tag
        // differs ("snapshot_stale" vs "high_water_mark").
        if (_depthSnapshot.HasObservation
            && !_depthSnapshot.IsFresh(_clock.UtcNow, _options.SnapshotMaxAgeSeconds))
        {
            throw new OutboxSaturatedException(
                observedDepth: _depthSnapshot.CurrentDepth,
                highWaterMark: _options.HighWaterMark,
                retryAfterSeconds: _options.RetryAfterSeconds,
                reason: "snapshot_stale");
        }

        if (_depthSnapshot.HasObservation && _depthSnapshot.CurrentDepth >= _options.HighWaterMark)
        {
            throw new OutboxSaturatedException(
                observedDepth: _depthSnapshot.CurrentDepth,
                highWaterMark: _options.HighWaterMark,
                retryAfterSeconds: _options.RetryAfterSeconds,
                reason: "high_water_mark");
        }

        // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): the outbox
        // INSERT loop now threads the request/host-shutdown CT into
        // BeginTransactionAsync, the per-row ExecuteNonQueryAsync, and
        // the final CommitAsync. The high-water-mark refusal path
        // (PC-3) and the SQL itself are unchanged.
        await using var conn = await _dataSource.Inner.OpenInstrumentedAsync(EventStoreDataSource.PoolName);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        for (var i = 0; i < events.Count; i++)
        {
            var domainEvent = events[i];
            var eventType = domainEvent.GetType().Name;
            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            var idempotencyKey = ComputeIdempotencyKey(correlationId, payload, i);
            var eventId = ComputeDeterministicId(correlationId, eventType, i);
            var aggregateId = ExtractAggregateId(domainEvent);

            await using var cmd = new NpgsqlCommand(
                """
                INSERT INTO outbox (id, correlation_id, event_id, aggregate_id, event_type, payload, idempotency_key, topic, status, created_at)
                VALUES (@id, @corrId, @eventId, @aggregateId, @evtType, @payload::jsonb, @idempKey, @topic, 'pending', NOW())
                ON CONFLICT (idempotency_key) DO NOTHING
                """,
                conn, tx);

            cmd.Parameters.AddWithValue("id", eventId);
            cmd.Parameters.AddWithValue("corrId", correlationId);
            cmd.Parameters.AddWithValue("eventId", eventId);
            cmd.Parameters.AddWithValue("aggregateId", aggregateId);
            cmd.Parameters.AddWithValue("evtType", eventType);
            cmd.Parameters.AddWithValue("payload", payload);
            cmd.Parameters.AddWithValue("idempKey", idempotencyKey);
            cmd.Parameters.AddWithValue("topic", topic);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    private static Guid ComputeDeterministicId(Guid correlationId, string eventType, int sequenceNumber)
    {
        var seed = $"{correlationId}:{eventType}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }

    /// <summary>
    /// Pulls AggregateId off a domain event by convention without taking a
    /// project reference on the domain layer. All domain events in this
    /// codebase carry an `AggregateId` property — either a raw <see cref="Guid"/>
    /// or a value-object wrapper exposing a `Value` property of type Guid.
    /// Returns Guid.Empty when neither shape is present — the column is NOT NULL
    /// in the outbox schema, so we never return null.
    /// </summary>
    private static Guid ExtractAggregateId(object domainEvent)
    {
        var prop = domainEvent.GetType().GetProperty("AggregateId");
        if (prop is null) return Guid.Empty;

        var value = prop.GetValue(domainEvent);
        if (value is null) return Guid.Empty;
        if (value is Guid g) return g;

        var valueProp = value.GetType().GetProperty("Value");
        if (valueProp?.GetValue(value) is Guid wrapped) return wrapped;

        return Guid.Empty;
    }

    private static string ComputeIdempotencyKey(Guid correlationId, string payload, int sequenceNumber)
    {
        var seed = $"{correlationId}:{payload}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }
}
