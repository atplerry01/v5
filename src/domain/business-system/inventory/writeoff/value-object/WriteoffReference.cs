namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public readonly record struct WriteoffReference
{
    public Guid Value { get; }

    public WriteoffReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WriteoffReference must not be empty — must reference a stock or item.", nameof(value));
        Value = value;
    }
}
