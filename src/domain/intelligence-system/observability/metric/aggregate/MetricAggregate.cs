namespace Whycespace.Domain.IntelligenceSystem.Observability.Metric;

public sealed class MetricAggregate
{
    public static MetricAggregate Create()
    {
        var aggregate = new MetricAggregate();
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
