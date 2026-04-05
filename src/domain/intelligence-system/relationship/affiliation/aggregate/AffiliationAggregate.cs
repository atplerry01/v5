namespace Whycespace.Domain.IntelligenceSystem.Relationship.Affiliation;

public sealed class AffiliationAggregate
{
    public static AffiliationAggregate Create()
    {
        var aggregate = new AffiliationAggregate();
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
