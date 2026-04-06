using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed record ResourceReservedEvent(
    ReservationId ReservationId,
    ResourceId ResourceId,
    decimal ReservedAmount,
    DateTimeOffset ExpiryTime
) : DomainEvent;
