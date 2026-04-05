namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class RuleAggregate
{
    public static RuleAggregate Create()
    {
        var aggregate = new RuleAggregate();
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
