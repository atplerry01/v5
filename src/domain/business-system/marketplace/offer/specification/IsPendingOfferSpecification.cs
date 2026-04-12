namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed class IsPendingOfferSpecification
{
    public bool IsSatisfiedBy(OfferStatus status)
    {
        return status == OfferStatus.Pending;
    }
}
