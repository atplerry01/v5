namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed class RoleAggregate
{
    public static RoleAggregate Create()
    {
        var aggregate = new RoleAggregate();
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
