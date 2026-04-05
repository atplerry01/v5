namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public sealed class MilestoneAggregate
{
    public static MilestoneAggregate Create()
    {
        var aggregate = new MilestoneAggregate();
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
