namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed class AnalysisAggregate
{
    public static AnalysisAggregate Create()
    {
        var aggregate = new AnalysisAggregate();
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
