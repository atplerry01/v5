namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed class CanWithdrawOfferSpecification
{
    public bool IsSatisfiedBy(OfferStatus status)
    {
        return status == OfferStatus.Pending;
    }
}
