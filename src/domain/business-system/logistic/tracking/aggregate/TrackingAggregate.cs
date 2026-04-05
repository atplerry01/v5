namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed class TrackingAggregate
{
    public static TrackingAggregate Create()
    {
        var aggregate = new TrackingAggregate();
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
