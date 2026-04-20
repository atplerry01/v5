namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(BundleStatus status, int memberCount)
        => status == BundleStatus.Draft && memberCount > 0;
}
