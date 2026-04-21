using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;

public static class EligibilityErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Eligibility has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Eligibility Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Eligibility Descriptor must not be empty.");
}
