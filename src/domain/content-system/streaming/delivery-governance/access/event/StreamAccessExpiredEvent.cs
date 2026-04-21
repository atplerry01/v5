using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessExpiredEvent(
    [property: JsonPropertyName("AggregateId")] StreamAccessId AccessId,
    Timestamp ExpiredAt) : DomainEvent;
