using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public readonly record struct LifecycleDescriptor
{
    public Guid ClusterReference { get; }
    public string LifecycleName { get; }

    public LifecycleDescriptor(Guid clusterReference, string lifecycleName)
    {
        Guard.Against(clusterReference == Guid.Empty, "LifecycleDescriptor cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(lifecycleName), "LifecycleDescriptor name must not be null or whitespace.");

        ClusterReference = clusterReference;
        LifecycleName = lifecycleName;
    }
}
