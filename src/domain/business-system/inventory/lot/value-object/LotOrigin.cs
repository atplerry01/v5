namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public readonly record struct LotOrigin
{
    public string Value { get; }

    public LotOrigin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LotOrigin must not be empty.", nameof(value));
        Value = value;
    }
}
