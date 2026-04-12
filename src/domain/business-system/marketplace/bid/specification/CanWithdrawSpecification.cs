namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public sealed class CanWithdrawSpecification
{
    public bool IsSatisfiedBy(BidStatus status)
    {
        return status == BidStatus.Placed;
    }
}
