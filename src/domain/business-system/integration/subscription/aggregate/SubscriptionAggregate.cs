namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public sealed class SubscriptionAggregate
{
    public static SubscriptionAggregate Create()
    {
        var aggregate = new SubscriptionAggregate();
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
