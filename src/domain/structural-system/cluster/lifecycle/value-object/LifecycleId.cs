namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public readonly record struct LifecycleId
{
    public Guid Value { get; }

    public LifecycleId(Guid value)
    {
        if (value == Guid.Empty)
            throw LifecycleErrors.MissingId();

        Value = value;
    }
}
