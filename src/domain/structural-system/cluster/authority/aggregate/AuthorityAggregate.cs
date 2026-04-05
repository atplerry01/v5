namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class AuthorityAggregate
{
    public static AuthorityAggregate Create()
    {
        var aggregate = new AuthorityAggregate();
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
