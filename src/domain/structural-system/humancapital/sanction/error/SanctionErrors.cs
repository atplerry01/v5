using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Sanction;

public static class SanctionErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Sanction has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Sanction Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Sanction Descriptor must not be empty.");
}
