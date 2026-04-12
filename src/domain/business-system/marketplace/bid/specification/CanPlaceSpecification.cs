namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public sealed class CanPlaceSpecification
{
    public bool IsSatisfiedBy(BidStatus status)
    {
        return status == BidStatus.Draft;
    }
}
