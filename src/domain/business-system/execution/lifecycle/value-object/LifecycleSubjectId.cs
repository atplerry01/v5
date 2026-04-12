namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public readonly record struct LifecycleSubjectId
{
    public Guid Value { get; }

    public LifecycleSubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LifecycleSubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}
