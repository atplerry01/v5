using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastCreatedEvent(
    [property: JsonPropertyName("AggregateId")] BroadcastId BroadcastId,
    StreamRef StreamRef,
    Timestamp CreatedAt) : DomainEvent;
