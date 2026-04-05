namespace Whycespace.Domain.IntelligenceSystem.Knowledge.Taxonomy;

public sealed class TaxonomyAggregate
{
    public static TaxonomyAggregate Create()
    {
        var aggregate = new TaxonomyAggregate();
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
