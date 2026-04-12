namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed class CanAcceptOfferSpecification
{
    public bool IsSatisfiedBy(OfferStatus status)
    {
        return status == OfferStatus.Pending;
    }
}
