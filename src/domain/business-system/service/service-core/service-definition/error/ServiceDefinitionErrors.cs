using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public static class ServiceDefinitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ServiceDefinitionId is required and must not be empty.");

    public static DomainException InvalidStateTransition(ServiceDefinitionStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ServiceDefinitionId id)
        => new DomainInvariantViolationException($"ServiceDefinition '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ServiceDefinition has already been initialized.");
}
