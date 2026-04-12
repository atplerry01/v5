namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public sealed record ReservationCreatedEvent(ReservationId ReservationId, ReservationItemId ItemId, int Quantity);
