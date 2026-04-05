namespace Whycespace.Domain.ConstitutionalSystem.Policy.Access;

public sealed class AccessAggregate
{
    public static AccessAggregate Create()
    {
        var aggregate = new AccessAggregate();
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
