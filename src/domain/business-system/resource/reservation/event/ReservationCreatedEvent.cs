namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed record ReservationCreatedEvent(
    ReservationId ReservationId,
    ResourceReference ResourceReference,
    ReservedCapacity ReservedCapacity);
