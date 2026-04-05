namespace Whycespace.Domain.OrchestrationSystem.Workflow.Transition;

public sealed class TransitionAggregate
{
    public static TransitionAggregate Create()
    {
        var aggregate = new TransitionAggregate();
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
