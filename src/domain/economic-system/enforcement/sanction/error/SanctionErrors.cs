using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Sanction;

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
}
