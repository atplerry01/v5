namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public readonly record struct TariffId
{
    public Guid Value { get; }

    public TariffId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TariffId value must not be empty.", nameof(value));

        Value = value;
    }
}
