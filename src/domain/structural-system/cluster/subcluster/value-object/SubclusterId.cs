namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public readonly record struct SubclusterId
{
    public Guid Value { get; }

    public SubclusterId(Guid value)
    {
        if (value == Guid.Empty)
            throw SubclusterErrors.MissingId();

        Value = value;
    }
}
