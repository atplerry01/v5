using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed record ReplayRequestedEvent(
    [property: JsonPropertyName("AggregateId")] ReplayId ReplayId,
    ArchiveRef ArchiveRef,
    ViewerRef ViewerRef,
    Timestamp RequestedAt) : DomainEvent;
