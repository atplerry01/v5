using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastResumedEvent(
    [property: JsonPropertyName("AggregateId")] BroadcastId BroadcastId,
    Timestamp ResumedAt) : DomainEvent;
