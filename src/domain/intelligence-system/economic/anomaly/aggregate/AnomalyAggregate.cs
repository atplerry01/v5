namespace Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;

public sealed class AnomalyAggregate
{
    public static AnomalyAggregate Create()
    {
        var aggregate = new AnomalyAggregate();
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
