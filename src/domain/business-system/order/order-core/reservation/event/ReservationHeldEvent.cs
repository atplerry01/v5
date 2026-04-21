using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed record ReservationHeldEvent(
    [property: JsonPropertyName("AggregateId")] ReservationId ReservationId,
    OrderRef Order,
    LineItemRef? LineItem,
    ReservationSubjectRef Subject,
    ReservationQuantity Quantity,
    ReservationExpiry Expiry,
    DateTimeOffset HeldAt) : DomainEvent;
