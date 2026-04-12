namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public readonly record struct ExecutionResourceId
{
    public Guid Value { get; }

    public ExecutionResourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExecutionResourceId value must not be empty.", nameof(value));
        Value = value;
    }
}
