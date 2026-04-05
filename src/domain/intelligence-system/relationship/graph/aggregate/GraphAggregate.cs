namespace Whycespace.Domain.IntelligenceSystem.Relationship.Graph;

public sealed class GraphAggregate
{
    public static GraphAggregate Create()
    {
        var aggregate = new GraphAggregate();
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
