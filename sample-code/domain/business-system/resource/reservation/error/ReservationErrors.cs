namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public static class ReservationErrors
{
    public const string ExceedsInventory = "Reservation amount exceeds available inventory.";
    public const string AlreadyExpired = "Reservation has already expired.";
    public const string AlreadyCancelled = "Reservation has already been cancelled.";
    public const string NotActive = "Reservation is not active.";
}
