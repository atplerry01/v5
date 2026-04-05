namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public sealed class UsageRightAggregate
{
    public static UsageRightAggregate Create()
    {
        var aggregate = new UsageRightAggregate();
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
