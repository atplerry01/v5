namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(ClusterStatus status) =>
        status == ClusterStatus.Active;
}
