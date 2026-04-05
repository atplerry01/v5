namespace Whycespace.Domain.IntelligenceSystem.Experiment.Cohort;

public sealed class CohortAggregate
{
    public static CohortAggregate Create()
    {
        var aggregate = new CohortAggregate();
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
