using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed record ReservationExpiredEvent(
    ReservationId ReservationId,
    ResourceId ResourceId
) : DomainEvent;
