namespace Whycespace.Domain.IntelligenceSystem.Observability.ChainMonitor;

public sealed class ChainMonitorAggregate
{
    public static ChainMonitorAggregate Create()
    {
        var aggregate = new ChainMonitorAggregate();
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
