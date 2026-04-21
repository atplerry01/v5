using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastScheduledEvent(
    [property: JsonPropertyName("AggregateId")] BroadcastId BroadcastId,
    BroadcastWindow Window,
    Timestamp ScheduledAt) : DomainEvent;
