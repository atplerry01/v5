using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentAcceptedEvent(
    [property: JsonPropertyName("AggregateId")] AmendmentId AmendmentId,
    DateTimeOffset AcceptedAt) : DomainEvent;
