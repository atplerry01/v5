namespace Whycespace.Domain.OrchestrationSystem.Workflow.Assignment;

public sealed class AssignmentAggregate
{
    public static AssignmentAggregate Create()
    {
        var aggregate = new AssignmentAggregate();
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
