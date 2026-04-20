namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationReleasedEvent(ReservationId ReservationId, DateTimeOffset ReleasedAt);
