namespace Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;

public sealed record RestrictionReadModel
{
    public Guid RestrictionId { get; init; }
    public Guid SubjectId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? RemovedAt { get; init; }
    public string RemovalReason { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }

    // Phase 7 B4 — cause-coupling projection. Populated from
    // RestrictionAppliedEvent.Cause (V2) or synthesized Legacy on V1.
    // CauseKind values: Sanction | PayoutFailure | CompensationFlow |
    // ComplianceViolation | Manual.
    public string CauseKind { get; init; } = string.Empty;
    public Guid? CauseReferenceId { get; init; }
    public string CauseDetail { get; init; } = string.Empty;

    // Phase 7 B4 — suspend / resume lifecycle projection.
    public DateTimeOffset? SuspendedAt { get; init; }
    public DateTimeOffset? ResumedAt { get; init; }
    public string SuspensionCauseKind { get; init; } = string.Empty;
    public Guid? SuspensionCauseReferenceId { get; init; }
    public string SuspensionCauseDetail { get; init; } = string.Empty;
}
