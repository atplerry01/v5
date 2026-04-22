using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.OrchestrationSystem.Workflow.Stage;

public sealed class StageAggregate : AggregateRoot
{
    public static StageAggregate Create()
    {
        var aggregate = new StageAggregate();
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
