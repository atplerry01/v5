using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed record ProgressTrackedEvent(
    [property: JsonPropertyName("AggregateId")] ProgressId ProgressId,
    SessionRef SessionRef,
    PlaybackPosition Position,
    Timestamp TrackedAt) : DomainEvent;
