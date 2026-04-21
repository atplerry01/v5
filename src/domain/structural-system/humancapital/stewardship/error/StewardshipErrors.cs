using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Stewardship;

public static class StewardshipErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Stewardship has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Stewardship Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Stewardship Descriptor must not be empty.");
}
