namespace Whycespace.Domain.OrchestrationSystem.Workflow.Step;

public sealed class StepAggregate
{
    public static StepAggregate Create()
    {
        var aggregate = new StepAggregate();
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
