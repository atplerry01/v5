namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ClusterStatus status) =>
        status == ClusterStatus.Defined;
}
