namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public readonly record struct OfferListingReference
{
    public Guid Value { get; }

    public OfferListingReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OfferListingReference value must not be empty.", nameof(value));
        Value = value;
    }
}
