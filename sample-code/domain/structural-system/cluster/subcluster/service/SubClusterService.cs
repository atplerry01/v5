namespace Whycespace.Domain.StructuralSystem.Cluster.SubCluster;

public sealed class SubClusterService
{
    public bool CanDeactivate(SubClusterAggregate subCluster, bool hasActiveSpvs)
    {
        return subCluster.Status == SubClusterStatus.Active && !hasActiveSpvs;
    }
}
