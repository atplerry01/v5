namespace Whycespace.Domain.IntelligenceSystem.Cost.CostVariance;

public sealed class CostVarianceAggregate
{
    public static CostVarianceAggregate Create()
    {
        var aggregate = new CostVarianceAggregate();
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
