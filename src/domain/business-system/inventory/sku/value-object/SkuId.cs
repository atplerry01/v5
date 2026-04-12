namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public readonly record struct SkuId
{
    public Guid Value { get; }

    public SkuId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SkuId value must not be empty.", nameof(value));
        Value = value;
    }
}
