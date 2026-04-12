namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed class CanConfirmBookingSpecification
{
    public bool IsSatisfiedBy(BookingStatus status)
    {
        return status == BookingStatus.Pending;
    }
}
