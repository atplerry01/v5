namespace Whycespace.Domain.IntelligenceSystem.Capacity.AllocationForecast;

public sealed class AllocationForecastAggregate
{
    public static AllocationForecastAggregate Create()
    {
        var aggregate = new AllocationForecastAggregate();
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
