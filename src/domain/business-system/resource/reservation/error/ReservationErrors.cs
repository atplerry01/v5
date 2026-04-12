namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public static class ReservationErrors
{
    public static ReservationDomainException MissingId()
        => new("ReservationId is required and must not be empty.");

    public static ReservationDomainException InvalidStateTransition(ReservationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ReservationDomainException ResourceReferenceRequired()
        => new("Reservation must reference a resource.");

    public static ReservationDomainException CapacityMustBePositive()
        => new("Reserved capacity must be greater than zero.");

    public static ReservationDomainException ExceedsAvailableCapacity(int requested, int available)
        => new($"Requested capacity '{requested}' exceeds available capacity '{available}'.");
}

public sealed class ReservationDomainException : Exception
{
    public ReservationDomainException(string message) : base(message) { }
}
