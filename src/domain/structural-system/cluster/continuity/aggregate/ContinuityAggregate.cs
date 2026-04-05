namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed class ContinuityAggregate
{
    public static ContinuityAggregate Create()
    {
        var aggregate = new ContinuityAggregate();
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
