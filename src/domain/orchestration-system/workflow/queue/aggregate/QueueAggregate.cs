namespace Whycespace.Domain.OrchestrationSystem.Workflow.Queue;

public sealed class QueueAggregate
{
    public static QueueAggregate Create()
    {
        var aggregate = new QueueAggregate();
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
