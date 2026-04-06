namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class ClusterException : DomainException
{
    public ClusterException(string message) : base("CLUSTER_ERROR", message) { }
}
