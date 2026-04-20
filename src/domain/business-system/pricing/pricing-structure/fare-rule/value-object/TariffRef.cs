namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public readonly record struct TariffRef
{
    public Guid Value { get; }

    public TariffRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TariffRef value must not be empty.", nameof(value));

        Value = value;
    }
}
