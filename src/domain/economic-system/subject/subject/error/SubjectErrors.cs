using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public static class SubjectErrors
{
    public static DomainException AlreadyRegistered(SubjectId subjectId) =>
        new($"Economic subject '{subjectId.Value}' is already registered.");

    public static DomainInvariantViolationException MissingStructuralRef() =>
        new("Invariant violated: economic subject must carry a structural reference.");

    public static DomainInvariantViolationException MissingEconomicRef() =>
        new("Invariant violated: economic subject must carry an economic reference.");
}
