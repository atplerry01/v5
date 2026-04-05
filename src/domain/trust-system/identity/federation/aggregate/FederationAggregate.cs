namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public sealed class FederationAggregate
{
    public static FederationAggregate Create()
    {
        var aggregate = new FederationAggregate();
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
