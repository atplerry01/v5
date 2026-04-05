namespace Whycespace.Domain.IntelligenceSystem.Experiment.Hypothesis;

public sealed class HypothesisAggregate
{
    public static HypothesisAggregate Create()
    {
        var aggregate = new HypothesisAggregate();
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
