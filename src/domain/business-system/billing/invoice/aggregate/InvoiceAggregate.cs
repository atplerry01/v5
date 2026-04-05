namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public sealed class InvoiceAggregate
{
    public static InvoiceAggregate Create()
    {
        var aggregate = new InvoiceAggregate();
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
