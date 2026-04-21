using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public static class ServiceOptionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ServiceOptionId is required and must not be empty.");

    public static DomainException MissingServiceDefinitionRef()
        => new DomainInvariantViolationException("ServiceOption must reference a service definition.");

    public static DomainException InvalidStateTransition(ServiceOptionStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ServiceOptionId id)
        => new DomainInvariantViolationException($"ServiceOption '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ServiceOption has already been initialized.");
}
