namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public readonly record struct SubscriptionId
{
    public Guid Value { get; }

    public SubscriptionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubscriptionId value must not be empty.", nameof(value));

        Value = value;
    }
}
