namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public readonly record struct SourcingRequestId
{
    public Guid Value { get; }
    public SourcingRequestId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SourcingRequestId value must not be empty.", nameof(value));
        Value = value;
    }
}
