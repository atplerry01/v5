namespace Whycespace.Domain.BusinessSystem.Agreement.Validity;

public readonly record struct ValidityId
{
    public Guid Value { get; }

    public ValidityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ValidityId value must not be empty.", nameof(value));

        Value = value;
    }
}
