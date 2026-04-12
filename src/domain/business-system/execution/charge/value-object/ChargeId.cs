namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public readonly record struct ChargeId
{
    public Guid Value { get; }

    public ChargeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChargeId value must not be empty.", nameof(value));
        Value = value;
    }
}
