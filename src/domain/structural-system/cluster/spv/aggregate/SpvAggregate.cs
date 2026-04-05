namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class SpvAggregate
{
    public static SpvAggregate Create()
    {
        var aggregate = new SpvAggregate();
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
