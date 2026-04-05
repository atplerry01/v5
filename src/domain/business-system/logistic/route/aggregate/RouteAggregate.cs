namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public sealed class RouteAggregate
{
    public static RouteAggregate Create()
    {
        var aggregate = new RouteAggregate();
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
