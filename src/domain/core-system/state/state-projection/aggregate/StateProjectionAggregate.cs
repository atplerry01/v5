namespace Whycespace.Domain.CoreSystem.State.StateProjection;

public sealed class StateProjectionAggregate
{
    public static StateProjectionAggregate Create()
    {
        var aggregate = new StateProjectionAggregate();
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
