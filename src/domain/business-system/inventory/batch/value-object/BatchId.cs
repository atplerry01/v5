namespace Whycespace.Domain.BusinessSystem.Inventory.Batch;

public readonly record struct BatchId
{
    public Guid Value { get; }

    public BatchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BatchId value must not be empty.", nameof(value));
        Value = value;
    }
}
