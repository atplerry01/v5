namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public readonly record struct OrderSourceReference
{
    public Guid Value { get; }

    public OrderSourceReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderSourceReference value must not be empty.", nameof(value));
        Value = value;
    }
}
