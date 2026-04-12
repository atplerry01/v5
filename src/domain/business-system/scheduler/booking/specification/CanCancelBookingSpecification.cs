namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed class CanCancelBookingSpecification
{
    public bool IsSatisfiedBy(BookingStatus status)
    {
        return status == BookingStatus.Pending || status == BookingStatus.Confirmed;
    }
}
