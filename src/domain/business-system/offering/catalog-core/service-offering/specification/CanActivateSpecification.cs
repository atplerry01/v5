namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ServiceOfferingStatus status) => status == ServiceOfferingStatus.Draft;
}
