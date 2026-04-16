using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whycespace.Runtime.EventFabric;
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
    private readonly EventSchemaRegistry _schemaRegistry;

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
        IClock clock,
        EventSchemaRegistry schemaRegistry)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(depthSnapshot);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(schemaRegistry);
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
        _schemaRegistry = schemaRegistry;
    }

    public async Task EnqueueAsync(
        Guid correlationId,
        Guid aggregateId,
        IReadOnlyList<object> events,
        string topic,
        CancellationToken cancellationToken = default)
    {
        // D9 / K-AGGREGATE-ID-HEADER-01 fail-fast at the outbox boundary.
        // Producer-side guarantee: the row stamped here is the same value the
        // KafkaOutboxPublisher will put on the message key + `aggregate-id`
        // header, so a Guid.Empty here would surface downstream as a key-zero
        // partition collision (R-K-11 / R-K-15) and a missing-header contract
        // violation (R-K-24). Reject at the entry rather than letting the
        // empty value flow further.
        if (aggregateId == Guid.Empty && events.Count > 0)
            throw new InvalidOperationException(
                "PostgresOutboxAdapter.EnqueueAsync received Guid.Empty aggregateId (D9 / K-AGGREGATE-ID-HEADER-01).");

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

            // E6.X/OUTBOX-PAYLOAD-ALIGNMENT: the outbox stores the SCHEMA-MAPPED
            // payload, not the raw domain event. Previously this adapter
            // serialised `domainEvent` directly, which shipped nested
            // value-object shapes (e.g. `"Currency": {"Code":"USD"}`) on the
            // Kafka wire. The projection consumer deserialises `eventType`
            // against the registered INBOUND SCHEMA type
            // (`CapitalAccountOpenedEventSchema`, flat fields), so nested
            // payloads produced a `JsonException` on every capital event and
            // the Kafka messages were correctly DLQ'd by the consumer (E12
            // projection-pipeline correction). The fix is to apply
            // `EventSchemaRegistry.MapPayload(eventType, domainEvent)` before
            // serialisation — the mapper returns the flat inbound schema
            // object when a mapper is registered for the event type, and
            // returns the input unchanged otherwise (no double-map: the
            // mapped object carries no registered mapper of its own). The
            // `event-type` column + Kafka header remain the DOMAIN event
            // class name so the consumer's registry lookup is preserved.
            var mappedPayload = _schemaRegistry.MapPayload(eventType, domainEvent);
            var payload = JsonSerializer.Serialize(mappedPayload, mappedPayload.GetType());

            var idempotencyKey = ComputeIdempotencyKey(correlationId, payload, i);
            var eventId = ComputeDeterministicId(correlationId, eventType, i);
            // D9 / K-AGGREGATE-ID-HEADER-01: the per-loop alias keeps the
            // existing parameter binding below readable. The outbox now uses
            // the envelope's authoritative aggregateId passed in by EventFabric
            // — no more reflection-by-property-name fallback.
            var rowAggregateId = aggregateId;

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
            cmd.Parameters.AddWithValue("aggregateId", rowAggregateId);
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

    private static string ComputeIdempotencyKey(Guid correlationId, string payload, int sequenceNumber)
    {
        var seed = $"{correlationId}:{payload}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }
}
