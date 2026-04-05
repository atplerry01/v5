namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public sealed class SetupAggregate
{
    public static SetupAggregate Create()
    {
        var aggregate = new SetupAggregate();
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
