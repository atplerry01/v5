namespace Whycespace.Domain.DecisionSystem.Governance.Sanction;

public sealed class SanctionAggregate
{
    public static SanctionAggregate Create()
    {
        var aggregate = new SanctionAggregate();
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
