namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public sealed class FailureAggregate
{
    public static FailureAggregate Create()
    {
        var aggregate = new FailureAggregate();
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
