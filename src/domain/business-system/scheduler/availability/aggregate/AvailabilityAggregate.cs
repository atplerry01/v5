namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed class AvailabilityAggregate
{
    public static AvailabilityAggregate Create()
    {
        var aggregate = new AvailabilityAggregate();
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
