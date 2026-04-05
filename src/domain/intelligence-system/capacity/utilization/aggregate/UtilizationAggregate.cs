namespace Whycespace.Domain.IntelligenceSystem.Capacity.Utilization;

public sealed class UtilizationAggregate
{
    public static UtilizationAggregate Create()
    {
        var aggregate = new UtilizationAggregate();
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
