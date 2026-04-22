using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed record ReplayPausedEvent(
    [property: JsonPropertyName("AggregateId")] ReplayId ReplayId,
    PlaybackPosition Position,
    Timestamp PausedAt) : DomainEvent;
