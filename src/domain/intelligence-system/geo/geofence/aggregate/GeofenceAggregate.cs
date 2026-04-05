namespace Whycespace.Domain.IntelligenceSystem.Geo.Geofence;

public sealed class GeofenceAggregate
{
    public static GeofenceAggregate Create()
    {
        var aggregate = new GeofenceAggregate();
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
