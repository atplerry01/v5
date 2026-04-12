namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public readonly record struct TransferQuantity
{
    public int Value { get; }

    public TransferQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("TransferQuantity must be positive.", nameof(value));
        Value = value;
    }
}
