namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public readonly record struct CatalogId
{
    public Guid Value { get; }

    public CatalogId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CatalogId value must not be empty.", nameof(value));

        Value = value;
    }
}
