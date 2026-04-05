namespace Whycespace.Domain.IntelligenceSystem.Economic.Forecast;

public sealed class ForecastAggregate
{
    public static ForecastAggregate Create()
    {
        var aggregate = new ForecastAggregate();
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
