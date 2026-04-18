namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

/// <summary>
/// Phase 7 T7.6 — classifies the underlying reason a lock was acquired.
/// Kind is immutable for the lifetime of a lock; suspension reasons
/// carry their own Kind without overwriting the original.
///
/// <para>
/// <c>Unknown</c> is the zero-value sentinel and is rejected by the
/// <see cref="EnforcementCause"/> factory.
/// </para>
///
/// Duplicated (not shared) with the Restriction sub-domain per the
/// existing per-context VO convention (<c>SubjectId</c>, <c>Reason</c>).
/// </summary>
public enum EnforcementCauseKind
{
    Unknown = 0,
    Sanction,
    PayoutFailure,
    CompensationFlow,
    ComplianceViolation,
    Manual
}
