namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed class LifecycleAggregate
{
    public static LifecycleAggregate Create()
    {
        var aggregate = new LifecycleAggregate();
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
