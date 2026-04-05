namespace Whycespace.Domain.OrchestrationSystem.Workflow.Checkpoint;

public sealed class CheckpointAggregate
{
    public static CheckpointAggregate Create()
    {
        var aggregate = new CheckpointAggregate();
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
