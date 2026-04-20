namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public readonly record struct BundleId
{
    public Guid Value { get; }

    public BundleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BundleId value must not be empty.", nameof(value));

        Value = value;
    }
}
