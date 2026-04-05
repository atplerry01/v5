namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public sealed class UsageAggregate
{
    public static UsageAggregate Create()
    {
        var aggregate = new UsageAggregate();
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
