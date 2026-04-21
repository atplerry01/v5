using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed record CancellationRequestedEvent(
    [property: JsonPropertyName("AggregateId")] CancellationId CancellationId,
    OrderRef Order,
    CancellationReason Reason,
    DateTimeOffset RequestedAt) : DomainEvent;
