using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed record PlaybackPositionUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ProgressId ProgressId,
    PlaybackPosition Position,
    Timestamp UpdatedAt) : DomainEvent;
