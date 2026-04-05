namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed class QuotaAggregate
{
    public static QuotaAggregate Create()
    {
        var aggregate = new QuotaAggregate();
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
