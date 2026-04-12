namespace Whycespace.Domain.BusinessSystem.Marketplace.Catalog;

public sealed class CanPublishSpecification
{
    public bool IsSatisfiedBy(CatalogStatus status)
    {
        return status == CatalogStatus.Draft;
    }
}
