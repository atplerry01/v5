namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public readonly record struct RenewalId
{
    public Guid Value { get; }

    public RenewalId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RenewalId value must not be empty.", nameof(value));

        Value = value;
    }
}
