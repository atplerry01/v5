namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public readonly record struct CostContextId
{
    public Guid Value { get; }

    public CostContextId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CostContextId value must not be empty.", nameof(value));
        Value = value;
    }
}
