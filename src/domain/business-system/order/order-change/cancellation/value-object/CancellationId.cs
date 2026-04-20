namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public readonly record struct CancellationId
{
    public Guid Value { get; }

    public CancellationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CancellationId value must not be empty.", nameof(value));

        Value = value;
    }
}
