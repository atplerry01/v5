using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects;

/// <summary>
/// R3.B.2 / D-R3B-4 / R-OUT-EFF-QUEUE-01..03 — Postgres-backed
/// <see cref="IOutboundEffectQueueStore"/>. Uses the shared
/// <see cref="EventStoreDataSource"/> pool so queue writes ride the same
/// durability guarantees as authoritative event writes. Claim semantics
/// match <c>KafkaOutboxPublisher</c>: <c>FOR UPDATE SKIP LOCKED</c> +
/// single-row claim update in the same transaction.
///
/// <para>Dedup: <see cref="InsertAsync"/> catches the <c>23505</c> unique
/// violation on <c>(provider_id, idempotency_key)</c> and surfaces it as
/// <see cref="OutboundEffectDuplicateKeyException"/>. The dispatcher's
/// <see cref="IOutboundEffectQueueStore.FindByIdempotencyKeyAsync"/>
/// pre-check handles the happy-path dedup; this constraint protects
/// against concurrent-writer races.</para>
///
/// <para><b>Payload storage (R3.B.3 / R-OUT-EFF-QUEUE-PAYLOAD-REGISTRY-01):</b>
/// JSONB with a <c>{TypeName, Json}</c> envelope where <c>TypeName</c> is
/// the canonical short name from <see cref="IPayloadTypeRegistry"/>.
/// Deserialization is <b>strict / fail-closed</b>: unknown type names throw
/// at read time rather than returning a typed null. Option (a) in the R3.B.3
/// decision log — no assembly-qualified-name fallback — so cross-assembly or
/// renamed-type drift surfaces as a loud failure, not a silent payload
/// corruption.</para>
/// </summary>
public sealed class PostgresOutboundEffectQueueStore : IOutboundEffectQueueStore
{
    private const string DuplicateKeyViolation = "23505";

    private readonly EventStoreDataSource _dataSource;
    private readonly IPayloadTypeRegistry _payloadTypes;
    private readonly ILogger<PostgresOutboundEffectQueueStore>? _logger;

    public PostgresOutboundEffectQueueStore(
        EventStoreDataSource dataSource,
        IPayloadTypeRegistry payloadTypes,
        ILogger<PostgresOutboundEffectQueueStore>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(payloadTypes);
        _dataSource = dataSource;
        _payloadTypes = payloadTypes;
        _logger = logger;
    }

    public async Task InsertAsync(
        OutboundEffectQueueEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await using var conn = await _dataSource.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO outbound_effect_dispatch_queue (
                effect_id, provider_id, effect_type, idempotency_key,
                status, attempt_count, max_attempts, next_attempt_at,
                dispatch_deadline, ack_deadline, finality_deadline,
                last_error, claimed_by, claimed_at, created_at, updated_at, payload
            ) VALUES (
                @effect_id, @provider_id, @effect_type, @idempotency_key,
                @status, @attempt_count, @max_attempts, @next_attempt_at,
                @dispatch_deadline, @ack_deadline, @finality_deadline,
                @last_error, NULL, NULL, @created_at, @updated_at, @payload
            )
            """,
            conn);
        BindEntryParameters(cmd, entry);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException pg) when (pg.SqlState == DuplicateKeyViolation)
        {
            throw new OutboundEffectDuplicateKeyException(entry.ProviderId, entry.IdempotencyKey);
        }
    }

    public async Task<Guid?> FindByIdempotencyKeyAsync(
        string providerId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT effect_id
            FROM outbound_effect_dispatch_queue
            WHERE provider_id = @provider_id AND idempotency_key = @idempotency_key
            LIMIT 1
            """,
            conn);
        cmd.Parameters.AddWithValue("provider_id", providerId);
        cmd.Parameters.AddWithValue("idempotency_key", idempotencyKey);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is Guid g ? g : null;
    }

    public async Task<OutboundEffectQueueEntry?> GetAsync(
        Guid effectId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT effect_id, provider_id, effect_type, idempotency_key, status,
                   attempt_count, max_attempts, next_attempt_at, dispatch_deadline,
                   ack_deadline, finality_deadline, last_error, claimed_by, claimed_at,
                   created_at, updated_at, payload
            FROM outbound_effect_dispatch_queue
            WHERE effect_id = @effect_id
            LIMIT 1
            """,
            conn);
        cmd.Parameters.AddWithValue("effect_id", effectId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return ReadEntry(reader);
    }

    public async Task<IReadOnlyList<OutboundEffectQueueEntry>> ClaimReadyAsync(
        string hostId,
        int maxBatch,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hostId))
            throw new ArgumentException("hostId is required.", nameof(hostId));
        if (maxBatch <= 0) return Array.Empty<OutboundEffectQueueEntry>();

        await using var conn = await _dataSource.OpenAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        // MI-2-equivalent: FOR UPDATE SKIP LOCKED makes claim disjoint across
        // concurrent hosts. Filter restricts to ready rows only.
        await using var selectCmd = new NpgsqlCommand(
            """
            SELECT effect_id, provider_id, effect_type, idempotency_key, status,
                   attempt_count, max_attempts, next_attempt_at, dispatch_deadline,
                   ack_deadline, finality_deadline, last_error, claimed_by, claimed_at,
                   created_at, updated_at, payload
            FROM outbound_effect_dispatch_queue
            WHERE claimed_by IS NULL
              AND status IN ('Scheduled', 'TransientFailed')
              AND next_attempt_at <= @now
            ORDER BY next_attempt_at ASC
            LIMIT @max_batch
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        selectCmd.Parameters.AddWithValue("now", now);
        selectCmd.Parameters.AddWithValue("max_batch", maxBatch);

        var rows = new List<OutboundEffectQueueEntry>();
        await using (var reader = await selectCmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(ReadEntry(reader));
            }
        }

        if (rows.Count == 0)
        {
            await tx.CommitAsync(cancellationToken);
            return rows;
        }

        await using var claimCmd = new NpgsqlCommand(
            """
            UPDATE outbound_effect_dispatch_queue
            SET claimed_by = @host, claimed_at = @now, updated_at = @now
            WHERE effect_id = ANY(@ids)
            """,
            conn, tx);
        claimCmd.Parameters.AddWithValue("host", hostId);
        claimCmd.Parameters.AddWithValue("now", now);
        claimCmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value =
            rows.Select(r => r.EffectId).ToArray();

        await claimCmd.ExecuteNonQueryAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return rows.Select(r => r with { ClaimedBy = hostId, ClaimedAt = now }).ToList();
    }

    public async Task UpdateStatusAsync(
        Guid effectId,
        string newStatus,
        int attemptCount,
        DateTimeOffset nextAttemptAt,
        DateTimeOffset? ackDeadline,
        DateTimeOffset? finalityDeadline,
        string? lastError,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE outbound_effect_dispatch_queue
            SET status = @status,
                attempt_count = @attempt_count,
                next_attempt_at = @next_attempt_at,
                ack_deadline = @ack_deadline,
                finality_deadline = @finality_deadline,
                last_error = @last_error,
                updated_at = @updated_at,
                claimed_by = NULL,
                claimed_at = NULL
            WHERE effect_id = @effect_id
            """,
            conn);
        cmd.Parameters.AddWithValue("effect_id", effectId);
        cmd.Parameters.AddWithValue("status", newStatus);
        cmd.Parameters.AddWithValue("attempt_count", attemptCount);
        cmd.Parameters.AddWithValue("next_attempt_at", nextAttemptAt);
        cmd.Parameters.AddWithValue("ack_deadline", (object?)ackDeadline ?? DBNull.Value);
        cmd.Parameters.AddWithValue("finality_deadline", (object?)finalityDeadline ?? DBNull.Value);
        cmd.Parameters.AddWithValue("last_error", (object?)lastError ?? DBNull.Value);
        cmd.Parameters.AddWithValue("updated_at", updatedAt);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OutboundEffectQueueEntry>> ClaimExpiredOrPollDueAsync(
        string hostId,
        int maxBatch,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hostId))
            throw new ArgumentException("hostId is required.", nameof(hostId));
        if (maxBatch <= 0) return Array.Empty<OutboundEffectQueueEntry>();

        await using var conn = await _dataSource.OpenAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        await using var selectCmd = new NpgsqlCommand(
            """
            SELECT effect_id, provider_id, effect_type, idempotency_key, status,
                   attempt_count, max_attempts, next_attempt_at, dispatch_deadline,
                   ack_deadline, finality_deadline, last_error, claimed_by, claimed_at,
                   created_at, updated_at, payload
            FROM outbound_effect_dispatch_queue
            WHERE claimed_by IS NULL
              AND (
                    (status = 'Dispatched'     AND ack_deadline      IS NOT NULL AND ack_deadline      <= @now)
                 OR (status = 'Acknowledged'   AND finality_deadline IS NOT NULL AND finality_deadline <= @now)
              )
            ORDER BY COALESCE(ack_deadline, finality_deadline) ASC
            LIMIT @max_batch
            FOR UPDATE SKIP LOCKED
            """,
            conn, tx);
        selectCmd.Parameters.AddWithValue("now", now);
        selectCmd.Parameters.AddWithValue("max_batch", maxBatch);

        var rows = new List<OutboundEffectQueueEntry>();
        await using (var reader = await selectCmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(ReadEntry(reader));
            }
        }

        if (rows.Count == 0)
        {
            await tx.CommitAsync(cancellationToken);
            return rows;
        }

        await using var claimCmd = new NpgsqlCommand(
            """
            UPDATE outbound_effect_dispatch_queue
            SET claimed_by = @host, claimed_at = @now, updated_at = @now
            WHERE effect_id = ANY(@ids)
            """,
            conn, tx);
        claimCmd.Parameters.AddWithValue("host", hostId);
        claimCmd.Parameters.AddWithValue("now", now);
        claimCmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value =
            rows.Select(r => r.EffectId).ToArray();

        await claimCmd.ExecuteNonQueryAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return rows.Select(r => r with { ClaimedBy = hostId, ClaimedAt = now }).ToList();
    }

    private void BindEntryParameters(NpgsqlCommand cmd, OutboundEffectQueueEntry entry)
    {
        cmd.Parameters.AddWithValue("effect_id", entry.EffectId);
        cmd.Parameters.AddWithValue("provider_id", entry.ProviderId);
        cmd.Parameters.AddWithValue("effect_type", entry.EffectType);
        cmd.Parameters.AddWithValue("idempotency_key", entry.IdempotencyKey);
        cmd.Parameters.AddWithValue("status", entry.Status);
        cmd.Parameters.AddWithValue("attempt_count", entry.AttemptCount);
        cmd.Parameters.AddWithValue("max_attempts", entry.MaxAttempts);
        cmd.Parameters.AddWithValue("next_attempt_at", entry.NextAttemptAt);
        cmd.Parameters.AddWithValue("dispatch_deadline", entry.DispatchDeadline);
        cmd.Parameters.AddWithValue("ack_deadline", (object?)entry.AckDeadline ?? DBNull.Value);
        cmd.Parameters.AddWithValue("finality_deadline", (object?)entry.FinalityDeadline ?? DBNull.Value);
        cmd.Parameters.AddWithValue("last_error", (object?)entry.LastError ?? DBNull.Value);
        cmd.Parameters.AddWithValue("created_at", entry.CreatedAt);
        cmd.Parameters.AddWithValue("updated_at", entry.UpdatedAt);

        var payloadJson = SerializePayload(entry.Payload);
        cmd.Parameters.Add("payload", NpgsqlDbType.Jsonb).Value = payloadJson;
    }

    private string SerializePayload(object payload)
    {
        // R-OUT-EFF-QUEUE-PAYLOAD-REGISTRY-01: resolve the canonical short
        // name via IPayloadTypeRegistry. Unregistered types fail closed.
        if (!_payloadTypes.TryGetName(payload.GetType(), out var typeName) || typeName is null)
        {
            throw new InvalidOperationException(
                $"Outbound-effect payload type '{payload.GetType().FullName}' is not registered " +
                "in IPayloadTypeRegistry. Register it from the owning domain's bootstrap module.");
        }

        var envelope = new PayloadEnvelope(
            TypeName: typeName,
            Json: JsonSerializer.Serialize(payload, payload.GetType()));
        return JsonSerializer.Serialize(envelope);
    }

    private object DeserializePayload(string json)
    {
        var envelope = JsonSerializer.Deserialize<PayloadEnvelope>(json)
            ?? throw new InvalidOperationException("Outbound-effect queue payload envelope is null.");

        // R-OUT-EFF-QUEUE-PAYLOAD-REGISTRY-01 / option (a): strict Resolve.
        // Unknown type names throw — no assembly-qualified-name fallback.
        var type = _payloadTypes.Resolve(envelope.TypeName);

        return JsonSerializer.Deserialize(envelope.Json, type)
            ?? throw new InvalidOperationException(
                $"Outbound-effect queue payload deserialized to null for type '{envelope.TypeName}'.");
    }

    private OutboundEffectQueueEntry ReadEntry(NpgsqlDataReader reader)
    {
        var payloadJson = reader.GetFieldValue<string>(16);
        return new OutboundEffectQueueEntry
        {
            EffectId = reader.GetGuid(0),
            ProviderId = reader.GetString(1),
            EffectType = reader.GetString(2),
            IdempotencyKey = reader.GetString(3),
            Status = reader.GetString(4),
            AttemptCount = reader.GetInt32(5),
            MaxAttempts = reader.GetInt32(6),
            NextAttemptAt = reader.GetFieldValue<DateTimeOffset>(7),
            DispatchDeadline = reader.GetFieldValue<DateTimeOffset>(8),
            AckDeadline = reader.IsDBNull(9) ? null : reader.GetFieldValue<DateTimeOffset>(9),
            FinalityDeadline = reader.IsDBNull(10) ? null : reader.GetFieldValue<DateTimeOffset>(10),
            LastError = reader.IsDBNull(11) ? null : reader.GetString(11),
            ClaimedBy = reader.IsDBNull(12) ? null : reader.GetString(12),
            ClaimedAt = reader.IsDBNull(13) ? null : reader.GetFieldValue<DateTimeOffset>(13),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(14),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(15),
            Payload = DeserializePayload(payloadJson),
        };
    }

    private sealed record PayloadEnvelope(string TypeName, string Json);
}
