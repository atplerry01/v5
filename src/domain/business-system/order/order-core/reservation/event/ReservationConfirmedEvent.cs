namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationConfirmedEvent(ReservationId ReservationId, DateTimeOffset ConfirmedAt);
