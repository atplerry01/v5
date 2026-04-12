namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public static class BookingErrors
{
    public static BookingDomainException MissingId()
        => new("BookingId is required and must not be empty.");

    public static BookingDomainException InvalidTimeRange()
        => new("Booking time range end must be after start.");

    public static BookingDomainException ConflictingBooking(BookingId id)
        => new($"Booking '{id.Value}' conflicts with an existing booking in the same time range.");

    public static BookingDomainException InvalidStateTransition(BookingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class BookingDomainException : Exception
{
    public BookingDomainException(string message) : base(message) { }
}
