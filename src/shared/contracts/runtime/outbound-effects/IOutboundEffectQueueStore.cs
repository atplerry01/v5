namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / D-R3B1-2 / R-OUT-EFF-QUEUE-01..03 — queue store contract. The
/// Postgres-backed concrete lives under
/// <c>src/platform/host/adapters/outbound-effects/</c>; an in-memory fake
/// serves tests. Inserts MUST be atomic with the aggregate's
/// <c>OutboundEffectScheduledEvent</c> append.
///
/// <para>Single-claim semantics: <see cref="ClaimReadyAsync"/> transitions
/// the selected rows' <c>claimed_by</c> column to the host id in an
/// <c>UPDATE ... WHERE claimed_by IS NULL</c> atomic write; concurrent
/// hosts see disjoint claim sets.</para>
/// </summary>
public interface IOutboundEffectQueueStore
{
    /// <summary>
    /// Insert a new queue row. Fails with <see cref="OutboundEffectDuplicateKeyException"/>
    /// if <c>(provider_id, idempotency_key)</c> already exists.
    /// </summary>
    Task InsertAsync(OutboundEffectQueueEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return the existing effect id for a given provider + idempotency key, or null.
    /// Used by the dispatcher's dedup check before attempting an insert.
    /// </summary>
    Task<Guid?> FindByIdempotencyKeyAsync(
        string providerId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<OutboundEffectQueueEntry?> GetAsync(
        Guid effectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically claim up to <paramref name="maxBatch"/> ready rows for the
    /// supplied <paramref name="hostId"/>. A row is "ready" when its status is
    /// <c>Scheduled</c> or <c>TransientFailed</c>, its <c>next_attempt_at</c>
    /// is due, and <c>claimed_by</c> is null.
    /// </summary>
    Task<IReadOnlyList<OutboundEffectQueueEntry>> ClaimReadyAsync(
        string hostId,
        int maxBatch,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update row status, attempt counters, deadlines, last-error text, and
    /// release the claim (<c>claimed_by := null</c>). Atomic single-row update.
    /// </summary>
    Task UpdateStatusAsync(
        Guid effectId,
        string newStatus,
        int attemptCount,
        DateTimeOffset nextAttemptAt,
        DateTimeOffset? ackDeadline,
        DateTimeOffset? finalityDeadline,
        string? lastError,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// R3.B.4 — claim rows whose ack or finality deadlines have elapsed, or
    /// whose finality poll is due. Matches rows where:
    /// (<c>status = Dispatched</c> AND <c>ack_deadline &lt;= now</c>) OR
    /// (<c>status = Acknowledged</c> AND <c>finality_deadline &lt;= now</c>).
    /// Same claim-by-host-id semantics as <see cref="ClaimReadyAsync"/>.
    /// </summary>
    Task<IReadOnlyList<OutboundEffectQueueEntry>> ClaimExpiredOrPollDueAsync(
        string hostId,
        int maxBatch,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// R3.B.1 — raised by <see cref="IOutboundEffectQueueStore.InsertAsync"/> when
/// the unique <c>(provider_id, idempotency_key)</c> constraint collides.
/// </summary>
public sealed class OutboundEffectDuplicateKeyException : Exception
{
    public OutboundEffectDuplicateKeyException(string providerId, string idempotencyKey)
        : base($"Duplicate outbound effect: providerId='{providerId}', idempotencyKey='{idempotencyKey}'.")
    {
        ProviderId = providerId;
        IdempotencyKey = idempotencyKey;
    }

    public string ProviderId { get; }
    public string IdempotencyKey { get; }
}
