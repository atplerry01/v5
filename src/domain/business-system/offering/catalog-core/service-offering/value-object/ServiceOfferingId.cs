namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public readonly record struct ServiceOfferingId
{
    public Guid Value { get; }

    public ServiceOfferingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceOfferingId value must not be empty.", nameof(value));

        Value = value;
    }
}
