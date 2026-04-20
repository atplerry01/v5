namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public readonly record struct OfferingPackageRef
{
    public Guid Value { get; }

    public OfferingPackageRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OfferingPackageRef value must not be empty.", nameof(value));

        Value = value;
    }
}
