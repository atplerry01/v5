namespace Whycespace.Domain.IntelligenceSystem.Geo.Proximity;

public sealed class ProximityAggregate
{
    public static ProximityAggregate Create()
    {
        var aggregate = new ProximityAggregate();
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
