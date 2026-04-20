namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public readonly record struct CatalogRef
{
    public Guid Value { get; }

    public CatalogRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CatalogRef value must not be empty.", nameof(value));

        Value = value;
    }
}
