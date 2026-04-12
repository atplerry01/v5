namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public readonly record struct SubclusterDescriptor
{
    public Guid ParentClusterReference { get; }
    public string SubclusterName { get; }

    public SubclusterDescriptor(Guid parentClusterReference, string subclusterName)
    {
        if (parentClusterReference == Guid.Empty)
            throw SubclusterErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(subclusterName))
            throw SubclusterErrors.MissingDescriptor();

        ParentClusterReference = parentClusterReference;
        SubclusterName = subclusterName;
    }
}
