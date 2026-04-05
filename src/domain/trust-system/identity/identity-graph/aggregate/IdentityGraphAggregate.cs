namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityGraphAggregate
{
    public static IdentityGraphAggregate Create()
    {
        var aggregate = new IdentityGraphAggregate();
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
