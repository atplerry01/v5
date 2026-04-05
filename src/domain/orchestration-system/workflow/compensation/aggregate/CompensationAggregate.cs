namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed class CompensationAggregate
{
    public static CompensationAggregate Create()
    {
        var aggregate = new CompensationAggregate();
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
