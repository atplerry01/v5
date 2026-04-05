namespace Whycespace.Domain.IntelligenceSystem.Relationship.Linkage;

public sealed class LinkageAggregate
{
    public static LinkageAggregate Create()
    {
        var aggregate = new LinkageAggregate();
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
