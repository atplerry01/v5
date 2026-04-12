namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public readonly record struct ClusterId
{
    public Guid Value { get; }

    public ClusterId(Guid value)
    {
        if (value == Guid.Empty)
            throw ClusterErrors.MissingId();

        Value = value;
    }
}
