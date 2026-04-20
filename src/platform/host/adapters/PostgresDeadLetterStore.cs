using Microsoft.Extensions.Logging;
using Npgsql;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.A.3b / R-DLQ-STORE-01 — PostgreSQL implementation of
/// <see cref="IDeadLetterStore"/>. Uses the declared event-store pool
/// (same connection strategy as <c>PostgresOutboxAdapter</c> and
/// <c>PostgresIdempotencyStoreAdapter</c>) so dead-letter writes ride
/// on the same durability guarantees as authoritative event writes.
///
/// Idempotency: <see cref="RecordAsync"/> uses <c>INSERT ... ON CONFLICT
/// (event_id) DO NOTHING</c> — the first writer wins, subsequent writers
/// no-op. Two concurrent consumers routing the same poison message
/// collapse to a single row deterministically.
///
/// Audit-preservation: <see cref="MarkReprocessedAsync"/> updates the
/// reprocessed_* columns; rows are NEVER hard-deleted. Retention lives
/// in a separate sweep job (R2.A.4).
/// </summary>
public sealed class PostgresDeadLetterStore : IDeadLetterStore
{
    private readonly EventStoreDataSource _dataSource;
    private readonly ILogger<PostgresDeadLetterStore>? _logger;

    public PostgresDeadLetterStore(
        EventStoreDataSource dataSource,
        ILogger<PostgresDeadLetterStore>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task RecordAsync(DeadLetterEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // R2.A.D.3c / R-POSTGRES-POOL-BREAKER-OPEN-SEMANTICS-01: best-effort
        // mirror. If the pool breaker is Open we log and return — the Kafka
        // DLQ topic publish in KafkaOutboxPublisher is the primary durability
        // path; the Postgres mirror is a secondary audit/read surface whose
        // unavailability must NOT block the DLQ route.
        NpgsqlConnection? acquired;
        try
        {
            acquired = await _dataSource.OpenAsync(cancellationToken);
        }
        catch (CircuitBreakerOpenException breakerEx)
        {
            _logger?.LogWarning(
                "PostgresDeadLetterStore.RecordAsync skipped: pool breaker '{Breaker}' open ({RetryAfter}s). eventId={EventId}.",
                breakerEx.BreakerName, breakerEx.RetryAfterSeconds, entry.EventId);
            return;
        }
        await using var conn = acquired;

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO dead_letter_entries (
                event_id, source_topic, event_type, correlation_id, causation_id,
                enqueued_at, failure_category, last_error, attempt_count,
                payload, schema_version
            )
            VALUES (
                @event_id, @source_topic, @event_type, @correlation_id, @causation_id,
                @enqueued_at, @failure_category, @last_error, @attempt_count,
                @payload, @schema_version
            )
            ON CONFLICT (event_id) DO NOTHING
            """,
            conn);

        cmd.Parameters.AddWithValue("event_id", entry.EventId);
        cmd.Parameters.AddWithValue("source_topic", entry.SourceTopic);
        cmd.Parameters.AddWithValue("event_type", entry.EventType);
        cmd.Parameters.AddWithValue("correlation_id", entry.CorrelationId);
        cmd.Parameters.AddWithValue("causation_id",
            (object?)entry.CausationId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("enqueued_at", entry.EnqueuedAt);
        cmd.Parameters.AddWithValue("failure_category",
            entry.FailureCategory is { } cat ? cat.ToString() : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("last_error", entry.LastError);
        cmd.Parameters.AddWithValue("attempt_count", entry.AttemptCount);
        cmd.Parameters.AddWithValue("payload", entry.Payload);
        cmd.Parameters.AddWithValue("schema_version",
            (object?)entry.SchemaVersion ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DeadLetterEntry?> GetAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT event_id, source_topic, event_type, correlation_id, causation_id,
                   enqueued_at, failure_category, last_error, attempt_count,
                   payload, schema_version, reprocessed_at, reprocessed_by_identity
            FROM dead_letter_entries
            WHERE event_id = @event_id
            LIMIT 1
            """,
            conn);
        cmd.Parameters.AddWithValue("event_id", eventId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return ReadEntry(reader);
    }

    public async Task<IReadOnlyList<DeadLetterEntry>> ListAsync(
        string sourceTopic,
        DateTimeOffset? since = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceTopic))
            throw new ArgumentException("sourceTopic is required.", nameof(sourceTopic));

        // Bound blast radius per R-DLQ-STORE-01: cap limit at 1000 regardless of caller.
        var effectiveLimit = Math.Min(Math.Max(1, limit), 1000);

        await using var conn = await _dataSource.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT event_id, source_topic, event_type, correlation_id, causation_id,
                   enqueued_at, failure_category, last_error, attempt_count,
                   payload, schema_version, reprocessed_at, reprocessed_by_identity
            FROM dead_letter_entries
            WHERE source_topic = @source_topic
              AND reprocessed_at IS NULL
              AND (@since IS NULL OR enqueued_at >= @since)
            ORDER BY enqueued_at DESC
            LIMIT @limit
            """,
            conn);
        cmd.Parameters.AddWithValue("source_topic", sourceTopic);
        cmd.Parameters.AddWithValue("since", (object?)since ?? DBNull.Value);
        cmd.Parameters.AddWithValue("limit", effectiveLimit);

        var results = new List<DeadLetterEntry>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadEntry(reader));
        }

        return results;
    }

    public async Task MarkReprocessedAsync(
        Guid eventId,
        string operatorIdentityId,
        DateTimeOffset reprocessedAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(operatorIdentityId))
            throw new ArgumentException("operatorIdentityId is required.", nameof(operatorIdentityId));

        await using var conn = await _dataSource.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            """
            UPDATE dead_letter_entries
            SET reprocessed_at = @reprocessed_at,
                reprocessed_by_identity = @operator_id
            WHERE event_id = @event_id
              AND reprocessed_at IS NULL
            """,
            conn);
        cmd.Parameters.AddWithValue("event_id", eventId);
        cmd.Parameters.AddWithValue("reprocessed_at", reprocessedAt);
        cmd.Parameters.AddWithValue("operator_id", operatorIdentityId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DeadLetterEntry ReadEntry(NpgsqlDataReader reader)
    {
        RuntimeFailureCategory? category = null;
        if (!reader.IsDBNull(6))
        {
            var raw = reader.GetString(6);
            if (Enum.TryParse<RuntimeFailureCategory>(raw, out var parsed))
                category = parsed;
            else
                category = RuntimeFailureCategory.Unknown;
        }

        return new DeadLetterEntry
        {
            EventId = reader.GetGuid(0),
            SourceTopic = reader.GetString(1),
            EventType = reader.GetString(2),
            CorrelationId = reader.GetGuid(3),
            CausationId = reader.IsDBNull(4) ? null : reader.GetGuid(4),
            EnqueuedAt = reader.GetFieldValue<DateTimeOffset>(5),
            FailureCategory = category,
            LastError = reader.GetString(7),
            AttemptCount = reader.GetInt32(8),
            Payload = (byte[])reader.GetValue(9),
            SchemaVersion = reader.IsDBNull(10) ? null : reader.GetInt32(10),
            ReprocessedAt = reader.IsDBNull(11) ? null : reader.GetFieldValue<DateTimeOffset>(11),
            ReprocessedByIdentityId = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }
}
