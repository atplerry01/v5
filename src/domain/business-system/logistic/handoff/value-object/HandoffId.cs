namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public readonly record struct HandoffId
{
    public Guid Value { get; }

    public HandoffId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HandoffId value must not be empty.", nameof(value));
        Value = value;
    }
}
