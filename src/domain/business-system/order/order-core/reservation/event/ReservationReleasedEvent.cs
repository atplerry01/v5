using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationReleasedEvent(
    [property: JsonPropertyName("AggregateId")] ReservationId ReservationId,
    DateTimeOffset ReleasedAt) : DomainEvent;
