using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Operator;

public static class OperatorErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Operator has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Operator Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Operator Descriptor must not be empty.");
}
