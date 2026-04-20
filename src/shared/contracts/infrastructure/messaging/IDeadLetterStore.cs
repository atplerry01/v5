using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Infrastructure.Messaging;

/// <summary>
/// R2.A.3a (R-DLQ-STORE-01) — canonical persistence contract for dead-letter
/// entries. Every message that lands on a <c>.deadletter</c> Kafka topic
/// MUST be mirrored here so operator inspection, re-drive controls (R4.B),
/// and retention sweeps (R2.A.4) can operate against queryable storage
/// rather than a Kafka offset.
///
/// A Kafka-only deadletter path is insufficient because:
/// <list type="number">
///   <item>operator inspection needs queryable storage;</item>
///   <item>re-drive controls operate against persisted entries;</item>
///   <item>retention/aging requires durable lineage.</item>
/// </list>
///
/// Implementations MUST be idempotent on <see cref="DeadLetterEntry.EventId"/> —
/// multi-instance consumers may both attempt to record the same poison
/// message. Duplicate records on the same EventId collapse to a single row.
/// </summary>
public interface IDeadLetterStore
{
    /// <summary>
    /// Persist a dead-letter entry. Idempotent on <see cref="DeadLetterEntry.EventId"/>:
    /// a second call with the same EventId is a no-op (the first record wins).
    /// </summary>
    Task RecordAsync(DeadLetterEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a specific entry by EventId. Returns <c>null</c> when no entry
    /// exists. Used by the R4 re-drive surface.
    /// </summary>
    Task<DeadLetterEntry?> GetAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query entries by source topic and optional time window. Used by the
    /// R4.B DLQ inspection API. Results ordered by <see cref="DeadLetterEntry.EnqueuedAt"/>
    /// descending. <paramref name="limit"/> is capped at 1000 by
    /// implementations to bound operator-query blast radius.
    /// </summary>
    Task<IReadOnlyList<DeadLetterEntry>> ListAsync(
        string sourceTopic,
        DateTimeOffset? since = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an entry as successfully re-driven (operator action, R4.B).
    /// Subsequent queries filter out re-driven entries by default unless
    /// the caller explicitly asks for them. Implementations preserve the
    /// row for audit history — NEVER hard-delete.
    /// </summary>
    Task MarkReprocessedAsync(
        Guid eventId,
        string operatorIdentityId,
        DateTimeOffset reprocessedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// R4.B — enumerate entries across ALL source topics. Used by the admin
    /// DLQ inspection surface when the operator wants to see every poison
    /// message, not just one topic. <paramref name="limit"/> is capped at
    /// 1000 by implementations. When <paramref name="includeReprocessed"/>
    /// is <c>false</c> (default) reprocessed rows are filtered out, matching
    /// <see cref="ListAsync(string, DateTimeOffset?, int, CancellationToken)"/>.
    /// </summary>
    Task<IReadOnlyList<DeadLetterEntry>> ListAllAsync(
        DateTimeOffset? since = null,
        int limit = 100,
        bool includeReprocessed = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Dead-letter entry shape. Every field is deterministic (timestamps
/// from <c>IClock</c>, identifiers from <c>IIdGenerator</c>). Payload is
/// raw bytes so replay / re-drive can re-deserialise with the original
/// schema version rather than losing shape in translation.
/// </summary>
public sealed record DeadLetterEntry
{
    public required Guid EventId { get; init; }

    /// <summary>
    /// Source topic the message arrived on — typically a `.events` or
    /// `.retry` tier topic. Preserved verbatim so re-drive routes back
    /// to the correct primary tier.
    /// </summary>
    public required string SourceTopic { get; init; }

    public required string EventType { get; init; }
    public required Guid CorrelationId { get; init; }
    public Guid? CausationId { get; init; }

    /// <summary>From <c>IClock.UtcNow</c> at the moment the message transitioned to deadletter.</summary>
    public required DateTimeOffset EnqueuedAt { get; init; }

    /// <summary>
    /// Canonical failure category per R1 Batch 3.5. Null only for poison
    /// messages where classification was not possible (deserialization
    /// failure at the envelope layer).
    /// </summary>
    public RuntimeFailureCategory? FailureCategory { get; init; }

    /// <summary>Safe, caller-facing error string. No stack traces, no internal type names.</summary>
    public required string LastError { get; init; }

    /// <summary>How many attempts preceded the deadletter transition (0 for poison messages routed directly).</summary>
    public required int AttemptCount { get; init; }

    /// <summary>Raw serialised payload. Not interpreted by the store.</summary>
    public required byte[] Payload { get; init; }

    /// <summary>
    /// Schema version of the payload, for re-drive translation when the
    /// schema has evolved. Null when the envelope carried no version.
    /// </summary>
    public int? SchemaVersion { get; init; }

    /// <summary>
    /// When non-null, the entry has been marked reprocessed via
    /// <see cref="IDeadLetterStore.MarkReprocessedAsync"/>. Subsequent queries
    /// filter these out by default.
    /// </summary>
    public DateTimeOffset? ReprocessedAt { get; init; }

    /// <summary>Operator identity that initiated the re-drive. Null until reprocessed.</summary>
    public string? ReprocessedByIdentityId { get; init; }
}
