namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public readonly record struct FailureTypeId
{
    public Guid Value { get; }

    public FailureTypeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FailureTypeId value must not be empty.", nameof(value));

        Value = value;
    }
}
