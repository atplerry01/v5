namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public readonly record struct LineItemRef
{
    public Guid Value { get; }

    public LineItemRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LineItemRef value must not be empty.", nameof(value));

        Value = value;
    }
}
