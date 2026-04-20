using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects;

/// <summary>
/// R3.B.1 / D-R3B1-2 — in-memory reference implementation of
/// <see cref="IOutboundEffectQueueStore"/>. Backs unit/integration tests of
/// the dispatcher and relay. The production Postgres-backed adapter lands in
/// R3.B.2 (first real adapter checkpoint) alongside the migration that lives
/// at <c>infrastructure/data/postgres/outbound-effects/migrations/001_outbound_effect_dispatch_queue.sql</c>.
///
/// <para>Atomicity note: the in-memory implementation serialises inserts with a
/// lock to honor the <c>(provider_id, idempotency_key)</c> uniqueness invariant
/// that the Postgres concrete enforces via a unique index.</para>
/// </summary>
public sealed class InMemoryOutboundEffectQueueStore : IOutboundEffectQueueStore
{
    private readonly ConcurrentDictionary<Guid, OutboundEffectQueueEntry> _byEffectId = new();
    private readonly ConcurrentDictionary<string, Guid> _byIdempotencyKey = new(StringComparer.Ordinal);
    private readonly object _insertLock = new();

    public Task InsertAsync(OutboundEffectQueueEntry entry, CancellationToken cancellationToken = default)
    {
        var key = Key(entry.ProviderId, entry.IdempotencyKey);
        lock (_insertLock)
        {
            if (_byIdempotencyKey.ContainsKey(key))
                throw new OutboundEffectDuplicateKeyException(entry.ProviderId, entry.IdempotencyKey);
            _byIdempotencyKey[key] = entry.EffectId;
            _byEffectId[entry.EffectId] = entry;
        }
        return Task.CompletedTask;
    }

    public Task<Guid?> FindByIdempotencyKeyAsync(
        string providerId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        Guid? result = _byIdempotencyKey.TryGetValue(Key(providerId, idempotencyKey), out var id) ? id : null;
        return Task.FromResult(result);
    }

    public Task<OutboundEffectQueueEntry?> GetAsync(
        Guid effectId,
        CancellationToken cancellationToken = default)
    {
        _byEffectId.TryGetValue(effectId, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<OutboundEffectQueueEntry>> ClaimReadyAsync(
        string hostId,
        int maxBatch,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var claimed = new List<OutboundEffectQueueEntry>();
        foreach (var kvp in _byEffectId)
        {
            if (claimed.Count >= maxBatch) break;
            var entry = kvp.Value;
            if (entry.ClaimedBy is not null) continue;
            if (entry.NextAttemptAt > now) continue;
            if (entry.Status is not OutboundEffectQueueStatus.Scheduled
                and not OutboundEffectQueueStatus.TransientFailed) continue;

            var claimedEntry = entry with { ClaimedBy = hostId, ClaimedAt = now };
            if (_byEffectId.TryUpdate(kvp.Key, claimedEntry, entry))
                claimed.Add(claimedEntry);
        }
        return Task.FromResult<IReadOnlyList<OutboundEffectQueueEntry>>(claimed);
    }

    public Task UpdateStatusAsync(
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
        _byEffectId.AddOrUpdate(
            effectId,
            _ => throw new InvalidOperationException($"EffectId {effectId} not found."),
            (_, existing) => existing with
            {
                Status = newStatus,
                AttemptCount = attemptCount,
                NextAttemptAt = nextAttemptAt,
                AckDeadline = ackDeadline,
                FinalityDeadline = finalityDeadline,
                LastError = lastError,
                UpdatedAt = updatedAt,
                ClaimedBy = null,
                ClaimedAt = null,
            });
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboundEffectQueueEntry>> ClaimExpiredOrPollDueAsync(
        string hostId,
        int maxBatch,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var claimed = new List<OutboundEffectQueueEntry>();
        foreach (var kvp in _byEffectId)
        {
            if (claimed.Count >= maxBatch) break;
            var entry = kvp.Value;
            if (entry.ClaimedBy is not null) continue;

            bool ackExpired =
                entry.Status == OutboundEffectQueueStatus.Dispatched
                && entry.AckDeadline is { } ack && ack <= now;
            bool finalityExpired =
                entry.Status == OutboundEffectQueueStatus.Acknowledged
                && entry.FinalityDeadline is { } fd && fd <= now;
            if (!ackExpired && !finalityExpired) continue;

            var claimedEntry = entry with { ClaimedBy = hostId, ClaimedAt = now };
            if (_byEffectId.TryUpdate(kvp.Key, claimedEntry, entry))
                claimed.Add(claimedEntry);
        }
        return Task.FromResult<IReadOnlyList<OutboundEffectQueueEntry>>(claimed);
    }

    private static string Key(string providerId, string idempotencyKey) =>
        $"{providerId}::{idempotencyKey}";
}
