namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed class CanRejectOfferSpecification
{
    public bool IsSatisfiedBy(OfferStatus status)
    {
        return status == OfferStatus.Pending;
    }
}
