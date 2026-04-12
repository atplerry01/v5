namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public static class ReservationErrors
{
    public static ReservationDomainException MissingId()
        => new("ReservationId is required and must not be empty.");

    public static ReservationDomainException MissingItemId()
        => new("ReservationItemId is required and must not be empty.");

    public static ReservationDomainException InvalidQuantity()
        => new("Reserved quantity must be greater than zero.");

    public static ReservationDomainException InvalidStateTransition(ReservationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ReservationDomainException AlreadyConfirmed(ReservationId id)
        => new($"Reservation '{id.Value}' has already been confirmed.");

    public static ReservationDomainException AlreadyReleased(ReservationId id)
        => new($"Reservation '{id.Value}' has already been released.");
}

public sealed class ReservationDomainException : Exception
{
    public ReservationDomainException(string message) : base(message) { }
}
