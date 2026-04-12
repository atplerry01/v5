namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public readonly record struct RetryPolicyId
{
    public Guid Value { get; }

    public RetryPolicyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RetryPolicyId value must not be empty.", nameof(value));

        Value = value;
    }
}
