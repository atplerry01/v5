using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.OrchestrationSystem.Workflow.Checkpoint;

public sealed class CheckpointAggregate : AggregateRoot
{
    public static CheckpointAggregate Create()
    {
        var aggregate = new CheckpointAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    protected override void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    protected override void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
