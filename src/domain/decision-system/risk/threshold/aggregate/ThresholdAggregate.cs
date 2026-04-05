namespace Whycespace.Domain.DecisionSystem.Risk.Threshold;

public sealed class ThresholdAggregate
{
    public static ThresholdAggregate Create()
    {
        var aggregate = new ThresholdAggregate();
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
