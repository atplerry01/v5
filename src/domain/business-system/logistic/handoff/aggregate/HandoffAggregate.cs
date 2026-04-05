namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public sealed class HandoffAggregate
{
    public static HandoffAggregate Create()
    {
        var aggregate = new HandoffAggregate();
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
