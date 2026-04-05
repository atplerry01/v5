namespace Whycespace.Domain.IntelligenceSystem.Knowledge.Ontology;

public sealed class OntologyAggregate
{
    public static OntologyAggregate Create()
    {
        var aggregate = new OntologyAggregate();
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
