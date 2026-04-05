namespace Whycespace.Domain.CoreSystem.State.SystemState;

public sealed class SystemStateAggregate
{
    public static SystemStateAggregate Create()
    {
        var aggregate = new SystemStateAggregate();
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
