namespace Whycespace.Domain.IntelligenceSystem.Knowledge.Reference;

public sealed class ReferenceAggregate
{
    public static ReferenceAggregate Create()
    {
        var aggregate = new ReferenceAggregate();
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
