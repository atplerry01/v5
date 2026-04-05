namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public sealed class PlanAggregate
{
    public static PlanAggregate Create()
    {
        var aggregate = new PlanAggregate();
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
