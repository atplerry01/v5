namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public sealed class CanDeactivateListingSpecification
{
    public bool IsSatisfiedBy(ListingStatus status)
    {
        return status == ListingStatus.Active;
    }
}
