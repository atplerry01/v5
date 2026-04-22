using Whycespace.Domain.SharedKernel.Primitives.Kernel;
namespace Whycespace.Domain.IntelligenceSystem.Experiment.ResultAnalysis;

public sealed class ResultAnalysisAggregate : AggregateRoot
{
    public static ResultAnalysisAggregate Create()
    {
        var aggregate = new ResultAnalysisAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    protected override void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    protected override void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
