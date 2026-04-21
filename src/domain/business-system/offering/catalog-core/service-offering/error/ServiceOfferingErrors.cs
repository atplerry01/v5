using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public static class ServiceOfferingErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ServiceOfferingId is required and must not be empty.");

    public static DomainException MissingServiceDefinition()
        => new DomainInvariantViolationException("ServiceDefinitionRef is required for a service-offering.");

    public static DomainException InvalidStateTransition(ServiceOfferingStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ServiceOfferingId id)
        => new DomainInvariantViolationException($"ServiceOffering '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ServiceOffering has already been initialized.");
}
