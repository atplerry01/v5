namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class AuthorizationAggregate
{
    public static AuthorizationAggregate Create()
    {
        var aggregate = new AuthorizationAggregate();
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
