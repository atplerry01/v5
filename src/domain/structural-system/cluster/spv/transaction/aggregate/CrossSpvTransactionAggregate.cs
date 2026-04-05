namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed class CrossSpvTransactionAggregate
{
    public static CrossSpvTransactionAggregate Create()
    {
        var aggregate = new CrossSpvTransactionAggregate();
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
