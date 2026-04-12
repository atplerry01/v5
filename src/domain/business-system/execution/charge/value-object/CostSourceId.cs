namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public readonly record struct CostSourceId
{
    public Guid Value { get; }

    public CostSourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CostSourceId value must not be empty.", nameof(value));
        Value = value;
    }
}
