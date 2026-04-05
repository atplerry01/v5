namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed class GrantAggregate
{
    public static GrantAggregate Create()
    {
        var aggregate = new GrantAggregate();
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
