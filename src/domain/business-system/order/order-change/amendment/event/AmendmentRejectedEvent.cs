using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentRejectedEvent(
    [property: JsonPropertyName("AggregateId")] AmendmentId AmendmentId,
    DateTimeOffset RejectedAt) : DomainEvent;
