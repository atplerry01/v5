namespace Whycespace.Domain.BusinessSystem.Marketplace.Catalog;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(CatalogStatus status)
    {
        return status == CatalogStatus.Published;
    }
}
