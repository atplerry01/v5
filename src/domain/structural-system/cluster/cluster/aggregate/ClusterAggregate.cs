namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class ClusterAggregate
{
    public static ClusterAggregate Create()
    {
        var aggregate = new ClusterAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
