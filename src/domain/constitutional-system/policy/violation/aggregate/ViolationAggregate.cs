namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

public sealed class ViolationAggregate
{
    public static ViolationAggregate Create()
    {
        var aggregate = new ViolationAggregate();
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
