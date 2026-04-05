namespace Whycespace.Domain.BusinessSystem.Agreement.Renewal;

public sealed class RenewalAggregate
{
    public static RenewalAggregate Create()
    {
        var aggregate = new RenewalAggregate();
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
