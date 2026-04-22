namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.6 — classifies the underlying reason an enforcement action
/// was taken. Kind is immutable for the lifetime of a restriction once
/// established at Apply-time; suspension reasons carry their own Kind
/// without overwriting the original.
///
/// <para>
/// <c>Unknown</c> is the zero-value sentinel and is rejected by the
/// <see cref="EnforcementCause"/> factory. No valid enforcement action
/// may carry it.
/// </para>
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
