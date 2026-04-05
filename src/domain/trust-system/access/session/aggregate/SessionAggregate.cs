namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed class SessionAggregate
{
    public static SessionAggregate Create()
    {
        var aggregate = new SessionAggregate();
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
