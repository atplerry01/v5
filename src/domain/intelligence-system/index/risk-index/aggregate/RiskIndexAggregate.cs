namespace Whycespace.Domain.IntelligenceSystem.Index.RiskIndex;

public sealed class RiskIndexAggregate
{
    public static RiskIndexAggregate Create()
    {
        var aggregate = new RiskIndexAggregate();
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
