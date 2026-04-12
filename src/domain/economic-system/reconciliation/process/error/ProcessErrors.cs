using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public static class ProcessErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException MissingId() =>
        new("ProcessId is required and must not be empty.");

    public static DomainException MissingLedgerReference() =>
        new("Reconciliation must reference a ledger source.");

    public static DomainException MissingObservedReference() =>
        new("Reconciliation must reference an observed source.");

    public static DomainException InvalidStateTransition(ReconciliationStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException NoResultProduced() =>
        new("Reconciliation must produce a result before resolution.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException InvalidStatus() =>
        new("Invariant violated: reconciliation status must be a defined value.");
}
