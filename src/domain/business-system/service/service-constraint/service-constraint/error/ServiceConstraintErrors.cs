using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public static class ServiceConstraintErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ServiceConstraintId is required and must not be empty.");

    public static DomainException MissingServiceDefinitionRef()
        => new DomainInvariantViolationException("ServiceConstraint must reference a service definition.");

    public static DomainException InvalidStateTransition(ConstraintStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ServiceConstraintId id)
        => new DomainInvariantViolationException($"ServiceConstraint '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ServiceConstraint has already been initialized.");
}
