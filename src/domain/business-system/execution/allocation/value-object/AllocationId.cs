namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public readonly record struct AllocationId
{
    public Guid Value { get; }

    public AllocationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AllocationId value must not be empty.", nameof(value));
        Value = value;
    }
}
