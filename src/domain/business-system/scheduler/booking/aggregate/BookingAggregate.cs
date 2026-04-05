namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed class BookingAggregate
{
    public static BookingAggregate Create()
    {
        var aggregate = new BookingAggregate();
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
