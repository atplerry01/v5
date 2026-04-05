namespace Whycespace.Domain.DecisionSystem.Governance.Mandate;

public sealed class MandateAggregate
{
    public static MandateAggregate Create()
    {
        var aggregate = new MandateAggregate();
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
