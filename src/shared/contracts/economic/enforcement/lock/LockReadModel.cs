namespace Whycespace.Shared.Contracts.Economic.Enforcement.Lock;

public sealed record LockReadModel
{
    public Guid LockId { get; init; }
    public Guid SubjectId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset LockedAt { get; init; }
    public DateTimeOffset? UnlockedAt { get; init; }
    public string UnlockReason { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }

    // Phase 7 B4 — cause-coupling projection. Populated from
    // SystemLockedEvent.Cause (V2) or synthesized Legacy on V1.
    public string CauseKind { get; init; } = string.Empty;
    public Guid? CauseReferenceId { get; init; }
    public string CauseDetail { get; init; } = string.Empty;

    // Phase 7 B4 / T7.9 — natural-expiry fields.
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset? ExpiredAt { get; init; }

    // Phase 7 B4 / T7.8 — suspend / resume lifecycle projection.
    public DateTimeOffset? SuspendedAt { get; init; }
    public DateTimeOffset? ResumedAt { get; init; }
    public string SuspensionCauseKind { get; init; } = string.Empty;
    public Guid? SuspensionCauseReferenceId { get; init; }
    public string SuspensionCauseDetail { get; init; } = string.Empty;
}
