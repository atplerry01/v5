using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public static class ServiceLevelErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ServiceLevelId is required and must not be empty.");

    public static DomainException MissingServiceDefinitionRef()
        => new DomainInvariantViolationException("ServiceLevel must reference a service definition.");

    public static DomainException InvalidStateTransition(ServiceLevelStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ServiceLevelId id)
        => new DomainInvariantViolationException($"ServiceLevel '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ServiceLevel has already been initialized.");
}
