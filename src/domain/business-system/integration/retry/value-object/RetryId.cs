namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public readonly record struct RetryId
{
    public Guid Value { get; }

    public RetryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RetryId value must not be empty.", nameof(value));

        Value = value;
    }
}
