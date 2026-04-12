namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public readonly record struct LotId
{
    public Guid Value { get; }

    public LotId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotId value must not be empty.", nameof(value));
        Value = value;
    }
}
