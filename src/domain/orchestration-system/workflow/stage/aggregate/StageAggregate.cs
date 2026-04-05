namespace Whycespace.Domain.OrchestrationSystem.Workflow.Stage;

public sealed class StageAggregate
{
    public static StageAggregate Create()
    {
        var aggregate = new StageAggregate();
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
