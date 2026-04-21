using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ObservabilityId ObservabilityId,
    Timestamp ArchivedAt) : DomainEvent;
