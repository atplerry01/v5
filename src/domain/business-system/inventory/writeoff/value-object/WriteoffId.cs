namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public readonly record struct WriteoffId
{
    public Guid Value { get; }

    public WriteoffId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WriteoffId value must not be empty.", nameof(value));
        Value = value;
    }
}
