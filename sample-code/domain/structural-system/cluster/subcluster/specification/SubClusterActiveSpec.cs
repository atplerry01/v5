namespace Whycespace.Domain.StructuralSystem.Cluster.SubCluster;

public sealed class SubClusterActiveSpec : Specification<SubClusterAggregate>
{
    public override bool IsSatisfiedBy(SubClusterAggregate entity)
    {
        return entity.Status == SubClusterStatus.Active;
    }
}
