namespace Whycespace.Domain.IntelligenceSystem.Geo.Routing;

public sealed class RoutingAggregate
{
    public static RoutingAggregate Create()
    {
        var aggregate = new RoutingAggregate();
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
