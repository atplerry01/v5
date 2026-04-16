using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Escalation;

public static class EscalationErrors
{
    public static DomainException MissingSubject() =>
        new("Escalation must reference a subject.");

    public static DomainException UnknownSeverity(string severity) =>
        new($"Unknown violation severity: '{severity}'.");

    public static DomainInvariantViolationException EmptySubject() =>
        new("Invariant violated: SubjectId cannot be empty.");

    public static DomainInvariantViolationException NegativeCounter() =>
        new("Invariant violated: ViolationCounter cannot be negative.");
}
