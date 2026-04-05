namespace Whycespace.Domain.BusinessSystem.Billing.Receivable;

public sealed class ReceivableAggregate
{
    public static ReceivableAggregate Create()
    {
        var aggregate = new ReceivableAggregate();
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
