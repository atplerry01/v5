namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(CatalogStatus status)
    {
        return status == CatalogStatus.Published;
    }
}
