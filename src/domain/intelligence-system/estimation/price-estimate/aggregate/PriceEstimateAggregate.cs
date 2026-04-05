namespace Whycespace.Domain.IntelligenceSystem.Estimation.PriceEstimate;

public sealed class PriceEstimateAggregate
{
    public static PriceEstimateAggregate Create()
    {
        var aggregate = new PriceEstimateAggregate();
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
