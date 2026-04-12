namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public readonly record struct SubscriptionTargetId
{
    public Guid Value { get; }

    public SubscriptionTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubscriptionTargetId value must not be empty.", nameof(value));

        Value = value;
    }
}
