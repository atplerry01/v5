namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public readonly record struct UsageId
{
    public Guid Value { get; }

    public UsageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UsageId value must not be empty.", nameof(value));

        Value = value;
    }
}
