namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Restriction;

/// <summary>
/// V1 wire shape: the positional constructor below. V2 (Phase 7 B4)
/// adds <see cref="Cause"/> as an init-only nullable property so
/// messages produced before T7.6 continue to deserialize with
/// <c>Cause = null</c>; new messages carry the explicit enforcement
/// cause coupling the restriction to its triggering aggregate.
/// </summary>
public sealed record RestrictionAppliedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset AppliedAt)
{
    public RestrictionEnforcementCauseDto? Cause { get; init; }
}

public sealed record RestrictionUpdatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string NewScope,
    string NewReason,
    DateTimeOffset UpdatedAt);

public sealed record RestrictionRemovedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string RemovalReason,
    DateTimeOffset RemovedAt);

/// <summary>
/// Phase 7 B4 / T7.7 — restriction suspended for a bounded cause
/// (typically a compensation flow). The SuspensionCause identifies the
/// triggering aggregate; the original Apply-time Cause on the aggregate
/// is preserved independently.
/// </summary>
public sealed record RestrictionSuspendedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    RestrictionEnforcementCauseDto SuspensionCause,
    DateTimeOffset SuspendedAt);

/// <summary>
/// Phase 7 B4 / T7.7 — restriction resumed from suspension. No new
/// cause is introduced; the aggregate restores its Apply-time state
/// exactly.
/// </summary>
public sealed record RestrictionResumedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    DateTimeOffset ResumedAt);

/// <summary>
/// Phase 7 B4 / T7.6 — wire-safe DTO for the domain
/// <c>EnforcementCause</c> value object (restriction sub-domain). Kind
/// is serialised as its enum name so V1 readers can still parse; the
/// valid values match <c>EnforcementCauseKind</c>:
/// <c>Sanction | PayoutFailure | CompensationFlow | ComplianceViolation | Manual</c>.
/// </summary>
public sealed record RestrictionEnforcementCauseDto(
    string Kind,
    Guid CauseReferenceId,
    string Detail);
