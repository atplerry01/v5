namespace Whycespace.Domain.BusinessSystem.Shared.Reference;

public readonly record struct OrderRef
{
    public Guid Value { get; }

    public OrderRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderRef value must not be empty.", nameof(value));

        Value = value;
    }
}
