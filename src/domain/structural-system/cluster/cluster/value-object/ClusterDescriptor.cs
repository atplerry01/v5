namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public readonly record struct ClusterDescriptor
{
    public string ClusterName { get; }
    public string ClusterType { get; }

    public ClusterDescriptor(string clusterName, string clusterType)
    {
        if (string.IsNullOrWhiteSpace(clusterName))
            throw ClusterErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(clusterType))
            throw ClusterErrors.MissingDescriptor();

        ClusterName = clusterName;
        ClusterType = clusterType;
    }
}
