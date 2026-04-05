namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public sealed class ProposalAggregate
{
    public static ProposalAggregate Create()
    {
        var aggregate = new ProposalAggregate();
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
