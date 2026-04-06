namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed class SubClusterException : DomainException
{
    public SubClusterException(string message) : base("SUBCLUSTER_ERROR", message) { }
}
