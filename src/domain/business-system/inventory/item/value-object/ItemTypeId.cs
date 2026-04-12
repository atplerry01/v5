namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public readonly record struct ItemTypeId
{
    public Guid Value { get; }

    public ItemTypeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ItemTypeId value must not be empty.", nameof(value));
        Value = value;
    }
}
