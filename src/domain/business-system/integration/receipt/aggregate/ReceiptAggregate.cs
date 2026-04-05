namespace Whycespace.Domain.BusinessSystem.Integration.Receipt;

public sealed class ReceiptAggregate
{
    public static ReceiptAggregate Create()
    {
        var aggregate = new ReceiptAggregate();
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
