namespace Whycespace.Domain.StructuralSystem.Cluster.Classification;

public sealed class ClassificationAggregate
{
    public static ClassificationAggregate Create()
    {
        var aggregate = new ClassificationAggregate();
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
