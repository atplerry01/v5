namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeId
{
    public Guid Value { get; }

    public SurchargeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SurchargeId value must not be empty.", nameof(value));

        Value = value;
    }
}
