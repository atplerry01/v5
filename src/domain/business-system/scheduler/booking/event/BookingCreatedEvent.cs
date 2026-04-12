namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed record BookingCreatedEvent(BookingId BookingId, BookingTimeRange TimeRange);
