namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public sealed class IsActiveListingSpecification
{
    public bool IsSatisfiedBy(ListingStatus status)
    {
        return status == ListingStatus.Active;
    }
}
