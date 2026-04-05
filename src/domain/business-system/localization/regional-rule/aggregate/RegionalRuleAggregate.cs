namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public sealed class RegionalRuleAggregate
{
    public static RegionalRuleAggregate Create()
    {
        var aggregate = new RegionalRuleAggregate();
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
