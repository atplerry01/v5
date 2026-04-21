using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationConfirmedEvent(
    [property: JsonPropertyName("AggregateId")] ReservationId ReservationId,
    DateTimeOffset ConfirmedAt) : DomainEvent;
