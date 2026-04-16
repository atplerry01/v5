using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Compliance;

public static class ComplianceErrors
{
    public static DomainException InvalidRule() => new("Compliance rule reference must be non-empty.");
    public static DomainException InvalidSubjectRef() => new("Compliance subject reference must be non-empty.");
    public static DomainException AlreadyPassed() => new("Compliance check already passed.");
    public static DomainException AlreadyFailed() => new("Compliance check already failed.");
    public static DomainException AlreadyExpired() => new("Compliance check already expired.");
    public static DomainException CannotTransitionFromTerminal(ComplianceCheckStatus s) =>
        new($"Compliance check cannot transition from terminal status {s}.");
    public static DomainInvariantViolationException SubjectMissing() =>
        new("Invariant violated: compliance check must reference a subject.");
}
