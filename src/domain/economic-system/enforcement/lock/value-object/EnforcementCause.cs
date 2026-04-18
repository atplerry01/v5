namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.6 — couples a lock to its underlying cause. Every
/// <see cref="LockAggregate"/> acquire or suspend carries exactly one
/// <see cref="EnforcementCause"/>, tying the lock to the triggering
/// aggregate so audit reconstruction is provable from events alone.
///
///   Kind=Sanction              CauseReferenceId=SanctionId
///   Kind=PayoutFailure         CauseReferenceId=PayoutId
///   Kind=CompensationFlow      CauseReferenceId=PayoutId|DistributionId
///   Kind=ComplianceViolation   CauseReferenceId=ViolationId
///   Kind=Manual                CauseReferenceId=ActorId
///
/// Duplicated (not shared) with the Restriction sub-domain per existing
/// per-context VO convention.
/// </summary>
public sealed record EnforcementCause
{
    public EnforcementCauseKind Kind { get; }
    public Guid CauseReferenceId { get; }
    public string Detail { get; }

    public EnforcementCause(EnforcementCauseKind kind, Guid causeReferenceId, string detail)
    {
        if (kind == EnforcementCauseKind.Unknown)
            throw new ArgumentException(
                "EnforcementCause requires a known Kind (Unknown is the invalid sentinel).",
                nameof(kind));
        if (causeReferenceId == Guid.Empty)
            throw new ArgumentException(
                "EnforcementCause requires a non-empty CauseReferenceId.",
                nameof(causeReferenceId));
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException(
                "EnforcementCause requires a non-empty Detail.",
                nameof(detail));

        Kind = kind;
        CauseReferenceId = causeReferenceId;
        Detail = detail;
    }

    /// <summary>
    /// Replay-only fallback for V1 event streams. See the Restriction
    /// sub-domain counterpart for full rationale.
    /// </summary>
    internal static EnforcementCause Legacy(Guid subjectId) =>
        new(EnforcementCauseKind.Manual, subjectId,
            "Legacy enforcement action — cause not recorded at lock-time (pre-T7.6 stream).");
}
