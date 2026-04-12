namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public readonly record struct ItemId
{
    public Guid Value { get; }

    public ItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ItemId value must not be empty.", nameof(value));
        Value = value;
    }
}
