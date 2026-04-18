using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.6 — command-side DTO for <c>EnforcementCause</c>. Stays
/// free of domain-type dependencies (commands must not reference the
/// domain VO directly); the handler maps this DTO into the validated
/// domain <c>EnforcementCause</c> at dispatch time.
/// </summary>
public sealed record EnforcementCauseDto(
    string Kind,
    Guid CauseReferenceId,
    string Detail);

public sealed record ApplyRestrictionCommand(
    Guid RestrictionId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset AppliedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;

    // Phase 7 T7.6 — optional on the wire for backward compatibility;
    // the handler synthesizes a Manual cause referencing SubjectId when
    // absent so the aggregate's non-null-cause invariant holds.
    public EnforcementCauseDto? Cause { get; init; }
}

public sealed record UpdateRestrictionCommand(
    Guid RestrictionId,
    string NewScope,
    string NewReason,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}

public sealed record RemoveRestrictionCommand(
    Guid RestrictionId,
    string RemovalReason,
    DateTimeOffset RemovedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}

// ── Phase 7 T7.7 — suspend/resume ────────────────────────────────

public sealed record SuspendRestrictionCommand(
    Guid RestrictionId,
    EnforcementCauseDto SuspensionCause,
    DateTimeOffset SuspendedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}

public sealed record ResumeRestrictionCommand(
    Guid RestrictionId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => RestrictionId;
}
