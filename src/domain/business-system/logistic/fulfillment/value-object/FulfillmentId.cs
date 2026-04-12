namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public readonly record struct FulfillmentId
{
    public Guid Value { get; }

    public FulfillmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FulfillmentId value must not be empty.", nameof(value));
        Value = value;
    }
}
