namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed class BookingSpecification
{
    public bool IsSatisfiedBy(BookingAggregate booking)
    {
        return booking.Id != default
            && booking.TimeRange.EndTicks > booking.TimeRange.StartTicks
            && Enum.IsDefined(booking.Status);
    }
}
