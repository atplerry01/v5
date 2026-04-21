using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public readonly record struct SubclusterDescriptor
{
    public ClusterRef ParentClusterReference { get; }
    public string SubclusterName { get; }

    public SubclusterDescriptor(ClusterRef parentClusterReference, string subclusterName)
    {
        Guard.Against(parentClusterReference == default, "SubclusterDescriptor parent cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(subclusterName), "SubclusterDescriptor name must not be null or whitespace.");

        ParentClusterReference = parentClusterReference;
        SubclusterName = subclusterName;
    }
}
