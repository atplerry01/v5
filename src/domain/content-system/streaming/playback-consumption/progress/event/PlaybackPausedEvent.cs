using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed record PlaybackPausedEvent(
    [property: JsonPropertyName("AggregateId")] ProgressId ProgressId,
    PlaybackPosition Position,
    Timestamp PausedAt) : DomainEvent;
