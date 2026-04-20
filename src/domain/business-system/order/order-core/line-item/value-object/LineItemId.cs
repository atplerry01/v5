namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public readonly record struct LineItemId
{
    public Guid Value { get; }

    public LineItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LineItemId value must not be empty.", nameof(value));

        Value = value;
    }
}
