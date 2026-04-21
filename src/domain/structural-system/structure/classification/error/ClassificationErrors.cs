using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public static class ClassificationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ClassificationId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("ClassificationDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(ClassificationStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Classification has already been initialized.");
}
