namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public sealed class ExecutionAggregate
{
    public static ExecutionAggregate Create()
    {
        var aggregate = new ExecutionAggregate();
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
