namespace Whycespace.Domain.CoreSystem.State.StateVersion;

public sealed class StateVersionAggregate
{
    public static StateVersionAggregate Create()
    {
        var aggregate = new StateVersionAggregate();
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
