using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ObservabilityId ObservabilityId,
    ObservabilitySnapshot PreviousSnapshot,
    ObservabilitySnapshot NewSnapshot,
    Timestamp UpdatedAt) : DomainEvent;
