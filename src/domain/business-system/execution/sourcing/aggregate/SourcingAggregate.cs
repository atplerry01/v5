namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed class SourcingAggregate
{
    public static SourcingAggregate Create()
    {
        var aggregate = new SourcingAggregate();
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
