namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public static class ServiceOfferingErrors
{
    public static ServiceOfferingDomainException MissingId()
        => new("ServiceOfferingId is required and must not be empty.");

    public static ServiceOfferingDomainException InvalidStateTransition(ServiceOfferingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ServiceOfferingDomainException ArchivedImmutable(ServiceOfferingId id)
        => new($"ServiceOffering '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ServiceOfferingDomainException : Exception
{
    public ServiceOfferingDomainException(string message) : base(message) { }
}
