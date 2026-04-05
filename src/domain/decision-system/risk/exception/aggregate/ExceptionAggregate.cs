namespace Whycespace.Domain.DecisionSystem.Risk.Exception;

public sealed class ExceptionAggregate
{
    public static ExceptionAggregate Create()
    {
        var aggregate = new ExceptionAggregate();
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
