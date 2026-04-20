namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public readonly record struct SurchargeAmount
{
    public decimal Value { get; }

    public SurchargeAmount(decimal value)
    {
        if (value < 0m)
            throw new ArgumentException("SurchargeAmount must be non-negative.", nameof(value));

        Value = value;
    }
}
