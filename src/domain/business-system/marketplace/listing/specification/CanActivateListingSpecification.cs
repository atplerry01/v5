namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public sealed class CanActivateListingSpecification
{
    public bool IsSatisfiedBy(ListingStatus status)
    {
        return status == ListingStatus.Draft || status == ListingStatus.Inactive;
    }
}
