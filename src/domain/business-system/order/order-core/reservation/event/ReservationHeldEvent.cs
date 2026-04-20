using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationHeldEvent(
    ReservationId ReservationId,
    OrderRef Order,
    LineItemRef? LineItem,
    ReservationSubjectRef Subject,
    ReservationQuantity Quantity,
    ReservationExpiry Expiry,
    DateTimeOffset HeldAt);
