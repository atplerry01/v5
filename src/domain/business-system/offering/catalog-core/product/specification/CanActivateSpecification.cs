namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProductStatus status) => status == ProductStatus.Draft;
}
