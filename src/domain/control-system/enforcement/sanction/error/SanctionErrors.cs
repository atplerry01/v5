using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

public static class SanctionErrors
{
    public static DomainException MissingSubjectReference() =>
        new("Sanction must reference a subject.");

    public static DomainException MissingReason() =>
        new("Sanction must include a reason.");

    public static DomainException AlreadyActive() =>
        new("Sanction is already active.");

    public static DomainException AlreadyRevoked() =>
        new("Sanction is already revoked.");

    public static DomainException AlreadyExpired() =>
        new("Sanction is already expired.");

    public static DomainException InvalidStateTransition(SanctionStatus current, string action) =>
        new($"Invalid state transition: cannot {action} a sanction in status {current}.");

    public static DomainException CannotRevokeTerminalSanction(SanctionStatus current) =>
        new($"Cannot revoke a sanction in terminal status {current}.");

    public static DomainException CannotExpireTerminalSanction(SanctionStatus current) =>
        new($"Cannot expire a sanction in terminal status {current}.");

    public static DomainInvariantViolationException EmptySanctionId() =>
        new("Invariant violated: SanctionId cannot be empty.");

    public static DomainInvariantViolationException OrphanSanction() =>
        new("Invariant violated: sanction must reference a subject.");

    // ── Phase 7 T7.10/T7.11 — enforcement linkage + lifecycle ─────────

    public static DomainException EnforcementRefRequired() =>
        new("Sanction activation requires a non-null EnforcementRef (Phase 7 T7.10).");

    public static DomainException EnforcementKindMismatch(SanctionType sanctionType, SanctionType refKind) =>
        new($"EnforcementRef.Kind ({refKind}) must match sanction Type ({sanctionType}). " +
            "Cross-kind activation is rejected to prevent sanction/enforcement drift.");

    public static DomainInvariantViolationException EnforcementMissingOnActive() =>
        new("Invariant violated: an Active sanction must carry a non-null EnforcementRef.");
}
