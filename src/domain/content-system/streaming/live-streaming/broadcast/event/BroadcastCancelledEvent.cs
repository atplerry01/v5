using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastCancelledEvent(
    [property: JsonPropertyName("AggregateId")] BroadcastId BroadcastId,
    string Reason,
    Timestamp CancelledAt) : DomainEvent;
