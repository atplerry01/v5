namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed class StateTransitionAggregate
{
    public static StateTransitionAggregate Create()
    {
        var aggregate = new StateTransitionAggregate();
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
