namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed class EntitlementGrantAggregate
{
    public static EntitlementGrantAggregate Create()
    {
        var aggregate = new EntitlementGrantAggregate();
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
