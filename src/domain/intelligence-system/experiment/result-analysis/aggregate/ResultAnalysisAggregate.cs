namespace Whycespace.Domain.IntelligenceSystem.Experiment.ResultAnalysis;

public sealed class ResultAnalysisAggregate
{
    public static ResultAnalysisAggregate Create()
    {
        var aggregate = new ResultAnalysisAggregate();
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
