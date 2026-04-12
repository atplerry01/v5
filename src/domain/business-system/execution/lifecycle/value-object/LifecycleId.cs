namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public readonly record struct LifecycleId
{
    public Guid Value { get; }

    public LifecycleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LifecycleId value must not be empty.", nameof(value));
        Value = value;
    }
}
