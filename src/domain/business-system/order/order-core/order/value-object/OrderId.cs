namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public readonly record struct OrderId
{
    public Guid Value { get; }

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderId value must not be empty.", nameof(value));
        Value = value;
    }
}
