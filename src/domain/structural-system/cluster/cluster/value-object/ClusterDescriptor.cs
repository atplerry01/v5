using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public readonly record struct ClusterDescriptor
{
    public string ClusterName { get; }
    public string ClusterType { get; }

    public ClusterDescriptor(string clusterName, string clusterType)
    {
        Guard.Against(string.IsNullOrWhiteSpace(clusterName), "ClusterDescriptor requires a non-empty ClusterName.");
        Guard.Against(string.IsNullOrWhiteSpace(clusterType), "ClusterDescriptor requires a non-empty ClusterType.");

        ClusterName = clusterName;
        ClusterType = clusterType;
    }
}
