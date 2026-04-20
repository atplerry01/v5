namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ProductStatus status) => status != ProductStatus.Archived;
}
