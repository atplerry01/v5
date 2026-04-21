using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastPausedEvent(
    [property: JsonPropertyName("AggregateId")] BroadcastId BroadcastId,
    Timestamp PausedAt) : DomainEvent;
