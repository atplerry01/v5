namespace Whycespace.Domain.IntelligenceSystem.Search.Ranking;

public sealed class RankingAggregate
{
    public static RankingAggregate Create()
    {
        var aggregate = new RankingAggregate();
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
