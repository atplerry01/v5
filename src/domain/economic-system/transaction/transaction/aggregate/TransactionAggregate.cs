namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class TransactionAggregate
{
    public static TransactionAggregate Create()
    {
        var aggregate = new TransactionAggregate();
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
