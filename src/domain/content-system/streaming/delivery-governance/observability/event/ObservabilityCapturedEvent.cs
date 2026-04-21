using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityCapturedEvent(
    [property: JsonPropertyName("AggregateId")] ObservabilityId ObservabilityId,
    StreamRef StreamRef,
    ArchiveRef? ArchiveRef,
    ObservabilityWindow Window,
    ObservabilitySnapshot Snapshot,
    Timestamp CapturedAt) : DomainEvent;
