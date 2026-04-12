namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public readonly record struct SkuCode
{
    public string Value { get; }

    public SkuCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SkuCode must not be empty.", nameof(value));
        Value = value;
    }
}
