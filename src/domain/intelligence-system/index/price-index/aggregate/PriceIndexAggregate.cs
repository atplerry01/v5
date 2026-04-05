namespace Whycespace.Domain.IntelligenceSystem.Index.PriceIndex;

public sealed class PriceIndexAggregate
{
    public static PriceIndexAggregate Create()
    {
        var aggregate = new PriceIndexAggregate();
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
