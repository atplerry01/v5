using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Governance;

public static class GovernanceErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Governance has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Governance Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Governance Descriptor must not be empty.");
}
