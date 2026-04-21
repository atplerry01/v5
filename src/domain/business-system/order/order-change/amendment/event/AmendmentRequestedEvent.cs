using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentRequestedEvent(
    [property: JsonPropertyName("AggregateId")] AmendmentId AmendmentId,
    OrderRef Order,
    AmendmentReason Reason,
    DateTimeOffset RequestedAt) : DomainEvent;
