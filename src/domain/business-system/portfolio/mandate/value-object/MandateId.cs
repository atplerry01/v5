namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public readonly record struct MandateId
{
    public Guid Value { get; }

    public MandateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MandateId value must not be empty.", nameof(value));

        Value = value;
    }
}
