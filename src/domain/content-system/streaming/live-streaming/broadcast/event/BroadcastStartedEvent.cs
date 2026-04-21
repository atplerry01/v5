using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastStartedEvent(
    [property: JsonPropertyName("AggregateId")] BroadcastId BroadcastId,
    Timestamp StartedAt) : DomainEvent;
