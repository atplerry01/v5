namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(CatalogStatus status)
    {
        return status == CatalogStatus.Draft;
    }
}
