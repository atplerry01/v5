namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed class NoConflictBookingSpecification
{
    public bool IsSatisfiedBy(BookingTimeRange proposed, IReadOnlyList<BookingTimeRange> existing)
    {
        for (var i = 0; i < existing.Count; i++)
        {
            if (proposed.OverlapsWith(existing[i]))
                return false;
        }

        return true;
    }
}
