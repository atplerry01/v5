namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public readonly record struct OfferId
{
    public Guid Value { get; }

    public OfferId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OfferId value must not be empty.", nameof(value));
        Value = value;
    }
}
