namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public sealed class PaymentApplicationAggregate
{
    public static PaymentApplicationAggregate Create()
    {
        var aggregate = new PaymentApplicationAggregate();
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
