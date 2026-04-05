namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public sealed class RetryAggregate
{
    public static RetryAggregate Create()
    {
        var aggregate = new RetryAggregate();
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
