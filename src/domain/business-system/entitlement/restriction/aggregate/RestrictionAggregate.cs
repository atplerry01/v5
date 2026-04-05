namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed class RestrictionAggregate
{
    public static RestrictionAggregate Create()
    {
        var aggregate = new RestrictionAggregate();
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
