namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ServiceOfferingStatus status) => status != ServiceOfferingStatus.Archived;
}
