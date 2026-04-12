namespace Whycespace.Domain.BusinessSystem.Agreement.Renewal;

public readonly record struct RenewalSourceId
{
    public Guid Value { get; }

    public RenewalSourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RenewalSourceId value must not be empty.", nameof(value));

        Value = value;
    }
}
