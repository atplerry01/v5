namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public readonly record struct ProductId
{
    public Guid Value { get; }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProductId value must not be empty.", nameof(value));

        Value = value;
    }
}
