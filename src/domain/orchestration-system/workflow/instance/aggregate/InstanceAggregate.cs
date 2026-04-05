namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public sealed class InstanceAggregate
{
    public static InstanceAggregate Create()
    {
        var aggregate = new InstanceAggregate();
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
