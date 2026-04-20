namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(BundleStatus status) => status != BundleStatus.Archived;
}
