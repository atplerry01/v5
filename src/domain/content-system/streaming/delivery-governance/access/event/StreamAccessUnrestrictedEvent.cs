using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessUnrestrictedEvent(
    [property: JsonPropertyName("AggregateId")] StreamAccessId AccessId,
    Timestamp UnrestrictedAt) : DomainEvent;
