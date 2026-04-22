namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.6 — couples an enforcement action to its underlying cause.
/// Every <see cref="RestrictionAggregate"/> apply, update, or suspend
/// carries exactly one <see cref="EnforcementCause"/>. This ties the
/// restriction to a specific triggering aggregate so the audit trail is
/// reconstructable from events alone:
///
///   Kind=Sanction              CauseReferenceId=SanctionId
///   Kind=PayoutFailure         CauseReferenceId=PayoutId
///   Kind=CompensationFlow      CauseReferenceId=PayoutId|DistributionId
///   Kind=ComplianceViolation   CauseReferenceId=ViolationId
///   Kind=Manual                CauseReferenceId=ActorId (operator-initiated)
///
/// <para>
/// Kind=Unknown and an empty <c>CauseReferenceId</c> are hard-rejected
/// so no enforcement action can be recorded without a provable cause.
/// </para>
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
    /// Replay-only fallback for V1 event streams that predate the
    /// cause-coupling contract. Synthesizes a Manual cause whose
    /// reference is the subject itself so the invariant "Cause non-null"
    /// stays satisfied without inventing a false triggering aggregate.
    /// NEW Apply / Lock paths MUST supply a real cause via the public
    /// constructor — this factory is internal and only invoked by the
    /// aggregate's Apply handler when reading legacy history.
    /// </summary>
    internal static EnforcementCause Legacy(Guid subjectId) =>
        new(EnforcementCauseKind.Manual, subjectId,
            "Legacy enforcement action — cause not recorded at apply-time (pre-T7.6 stream).");
}
