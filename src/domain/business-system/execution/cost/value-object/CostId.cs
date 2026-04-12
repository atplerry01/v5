namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public readonly record struct CostId
{
    public Guid Value { get; }

    public CostId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CostId value must not be empty.", nameof(value));
        Value = value;
    }
}
