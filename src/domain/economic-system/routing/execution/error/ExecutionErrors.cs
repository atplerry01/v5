using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public static class ExecutionErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException InvalidPathReference() =>
        new("Execution must reference a valid routing path.");

    public static DomainException ReasonMustBeProvided() =>
        new("A reason must be provided for failure or abortion.");

    public static DomainException InvalidStateTransition(ExecutionStatus current, ExecutionStatus target) =>
        new($"Cannot transition execution from '{current}' to '{target}'.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException PathIdMustNotBeEmpty() =>
        new("Invariant violated: execution must reference a non-empty PathId.");

    public static DomainInvariantViolationException TerminalBeforeStart() =>
        new("Invariant violated: terminal timestamp cannot precede start timestamp.");
}
