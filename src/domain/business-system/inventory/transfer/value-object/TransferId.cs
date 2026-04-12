namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public readonly record struct TransferId
{
    public Guid Value { get; }

    public TransferId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransferId value must not be empty.", nameof(value));
        Value = value;
    }
}
