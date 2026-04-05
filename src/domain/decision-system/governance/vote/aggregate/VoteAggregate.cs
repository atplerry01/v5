namespace Whycespace.Domain.DecisionSystem.Governance.Vote;

public sealed class VoteAggregate
{
    public static VoteAggregate Create()
    {
        var aggregate = new VoteAggregate();
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
