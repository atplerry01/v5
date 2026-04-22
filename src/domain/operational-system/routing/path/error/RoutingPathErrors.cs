using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Path;

public static class RoutingPathErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException InvalidPathCondition() =>
        new("Path conditions must not be null or empty.");

    public static DomainException InvalidPriority(int priority) =>
        new($"Priority must be greater than zero. Received: {priority}.");

    public static DomainException InvalidStateTransition(RoutingPathStatus current, RoutingPathStatus target) =>
        new($"Cannot transition from '{current}' to '{target}'.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException ConditionsMustNotBeEmpty() =>
        new("Invariant violated: path conditions must not be empty.");

    public static DomainInvariantViolationException PriorityMustBePositive() =>
        new("Invariant violated: path priority must be greater than zero.");
}
