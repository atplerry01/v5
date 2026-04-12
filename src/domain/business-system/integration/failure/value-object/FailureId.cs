namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public readonly record struct FailureId
{
    public Guid Value { get; }

    public FailureId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FailureId value must not be empty.", nameof(value));

        Value = value;
    }
}
