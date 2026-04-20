namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public readonly record struct RateCardId
{
    public Guid Value { get; }

    public RateCardId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RateCardId value must not be empty.", nameof(value));

        Value = value;
    }
}
