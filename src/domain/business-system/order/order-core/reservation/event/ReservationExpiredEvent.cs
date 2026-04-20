namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationExpiredEvent(ReservationId ReservationId, DateTimeOffset ExpiredAt);
