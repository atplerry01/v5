namespace Whycespace.Domain.DecisionSystem.Risk.Control;

public sealed class ControlAggregate
{
    public static ControlAggregate Create()
    {
        var aggregate = new ControlAggregate();
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
