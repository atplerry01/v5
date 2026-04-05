namespace Whycespace.Domain.IntelligenceSystem.Simulation.Recommendation;

public sealed class RecommendationAggregate
{
    public static RecommendationAggregate Create()
    {
        var aggregate = new RecommendationAggregate();
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
