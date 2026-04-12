namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public readonly record struct SourcingId
{
    public Guid Value { get; }
    public SourcingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SourcingId value must not be empty.", nameof(value));
        Value = value;
    }
}
