using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed record CancellationRejectedEvent(
    [property: JsonPropertyName("AggregateId")] CancellationId CancellationId,
    DateTimeOffset RejectedAt) : DomainEvent;
