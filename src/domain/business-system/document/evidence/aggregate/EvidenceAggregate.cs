namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed class EvidenceAggregate
{
    public static EvidenceAggregate Create()
    {
        var aggregate = new EvidenceAggregate();
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
