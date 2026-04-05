namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed class SubclusterAggregate
{
    public static SubclusterAggregate Create()
    {
        var aggregate = new SubclusterAggregate();
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
