namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public readonly record struct LifecycleDescriptor
{
    public Guid ClusterReference { get; }
    public string LifecycleName { get; }

    public LifecycleDescriptor(Guid clusterReference, string lifecycleName)
    {
        if (clusterReference == Guid.Empty)
            throw LifecycleErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(lifecycleName))
            throw LifecycleErrors.MissingDescriptor();

        ClusterReference = clusterReference;
        LifecycleName = lifecycleName;
    }
}
