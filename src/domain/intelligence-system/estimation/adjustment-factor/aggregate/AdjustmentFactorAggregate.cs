namespace Whycespace.Domain.IntelligenceSystem.Estimation.AdjustmentFactor;

public sealed class AdjustmentFactorAggregate
{
    public static AdjustmentFactorAggregate Create()
    {
        var aggregate = new AdjustmentFactorAggregate();
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
