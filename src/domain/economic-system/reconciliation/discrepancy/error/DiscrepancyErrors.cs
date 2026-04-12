using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public static class DiscrepancyErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException MissingId() =>
        new("DiscrepancyId is required and must not be empty.");

    public static DomainException MissingProcessReference() =>
        new("Discrepancy must reference a reconciliation process.");

    public static DomainException EmptyResolution() =>
        new("Resolution description must not be empty.");

    public static DomainException InvalidStateTransition(DiscrepancyStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException InvalidStatus() =>
        new("Invariant violated: discrepancy status must be a defined value.");
}
