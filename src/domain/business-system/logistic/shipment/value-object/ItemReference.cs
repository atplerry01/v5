namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public readonly record struct ItemReference
{
    public Guid Value { get; }

    public ItemReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ItemReference value must not be empty.", nameof(value));

        Value = value;
    }
}
