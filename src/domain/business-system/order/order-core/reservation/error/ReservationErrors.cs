namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public static class ReservationErrors
{
    public static ReservationDomainException MissingId()
        => new("ReservationId is required and must not be empty.");

    public static ReservationDomainException MissingOrderRef()
        => new("Reservation must reference an order.");

    public static ReservationDomainException InvalidStateTransition(ReservationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ReservationDomainException AlreadyTerminal(ReservationId id, ReservationStatus status)
        => new($"Reservation '{id.Value}' is already terminal ({status}).");

    public static ReservationDomainException ExpiryInPast()
        => new("ReservationExpiry cannot already be in the past at the moment of creation.");

    public static ReservationDomainException ExpiryNotReached()
        => new("Reservation cannot be marked expired before its expiry moment.");
}

public sealed class ReservationDomainException : Exception
{
    public ReservationDomainException(string message) : base(message) { }
}
